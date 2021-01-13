﻿using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Xamarin.iOS.Tasks.Windows.Properties;
using Xamarin.Messaging.Build.Client;
using Xamarin.Messaging.Build.Contracts;

namespace Xamarin.iOS.Tasks.Windows {
	public class ResolveUniversalTypeIdentifiers : Task {
		[Required]
		public ITaskItem [] ImageAssets { get; set; }

		[Required]
		public string ProjectDir { get; set; }

		[Required]
		public string SessionId { get; set; }

		public override bool Execute ()
		{
			try {
				LogTaskProperty ("ImageAssets", ImageAssets);
				LogTaskProperty ("ProjectDir", ProjectDir);
				LogTaskProperty ("SessionId", SessionId);

				var connection = BuildConnection.GetAsync (SessionId, BuildEngine4).Result;
				var buildClient = connection.Client as BuildClient;

				if (!connection.IsConnected || buildClient == null) {
					Log.LogWarning (Resources.Task_NoConnectionAvailable, nameof (ResolveUniversalTypeIdentifiers));

					return true;
				}

				var contentFiles = ImageAssets
					.Where (asset => Path.GetFileName (asset.ItemSpec) == "Contents.json" &&
						Path.GetDirectoryName (asset.ItemSpec).EndsWith (".dataset"))
					.Select (x => x.ItemSpec);

				foreach (var filePath in contentFiles) {
					string content;
					DataSet dataset = null;

					if (File.Exists (filePath)) {
						content = File.ReadAllText (filePath);
						dataset = JsonConvert.DeserializeObject<DataSet> (content);
					}

					if (dataset == null) {
						Log.LogError (Resources.ResolveUniversalTypeIdentifiers_Deserialize_Error, Path.GetDirectoryName (filePath));
						continue;
					}

					var dataItemsToComplete = dataset
						.DataItems
						.Where (item => string.IsNullOrEmpty (item.UniversalTypeIdentifier) &&
							!string.IsNullOrEmpty (item.Filename)).ToList ();

					foreach (var data in dataItemsToComplete) {
						var file = ImageAssets.FirstOrDefault (x => Path.GetFileName (x.ItemSpec) == data.Filename);

						if (file == null) {
							Log.LogWarning (Resources.ResolveUniversalTypeIdentifiers_NoImageSet, data.Filename);

							continue;
						}

						Log.LogMessage (MessageImportance.Normal, "", Resources.ResolveUniversalTypeIdentifiers_GetUniversalTypeIdentifier, buildClient.AppName);

						var message = new GetUniversalTypeIdentifierMessage {
							Payload = File.ReadAllBytes (file.ItemSpec),
							FileName = Path.GetFileName (data.Filename)
						};

						var response = buildClient
							.RunMessagingAsync<GetUniversalTypeIdentifierMessage, GetUniversalTypeIdentifierResult> (message, timeoutSecs: 10)
							.Result;

						if (string.IsNullOrEmpty (response.UniversalTypeIdentifier))
							Log.LogError (string.Format (Resources.ResolveUniversalTypeIdentifiers_UTI_Error, data.Filename, Path.GetDirectoryName (filePath)));

						data.UniversalTypeIdentifier = response.UniversalTypeIdentifier;
					}

					if (dataItemsToComplete.Any ()) {
						content = JsonConvert.SerializeObject (dataset, Formatting.Indented);
						File.WriteAllText (filePath, content);
					}
				}

				return !Log.HasLoggedErrors;
			} catch (Exception ex) {
				Log.LogErrorFromException (ex);

				return false;
			}
		}

		//TODO: Ideally we should get this from the LoggingExtensions in Xamarin.MacDev.Tasks. We would need the reference for that

		void LogTaskProperty (string propertyName, ITaskItem [] items)
		{
			if (items == null) {
				Log.LogMessage (MessageImportance.Normal, "  {0}: <null>", propertyName);

				return;
			}

			Log.LogMessage (MessageImportance.Normal, "  {0}:", propertyName);

			for (int i = 0; i < items.Length; i++)
				Log.LogMessage (MessageImportance.Normal, "    {0}", items [i].ItemSpec);
		}

		void LogTaskProperty (string propertyName, string value)
		{
			Log.LogMessage (MessageImportance.Normal, "  {0}: {1}", propertyName, value ?? "<null>");
		}
	}
}
