//
// AutomaticAssessmentConfiguration C# bindings
//
// Authors:
//	Alex Soto <alexsoto@microsoft.com>
//	TJ Lambert <TJ.Lambert@microsoft.com>
//
// Copyright (c) Microsoft Corporation.
//

using System;
using Foundation;
using ObjCRuntime;

namespace AutomaticAssessmentConfiguration {

	[ErrorDomain ("AEAssessmentErrorDomain")]
	[Mac (10,15,4), iOS (13,4)]
	[MacCatalyst (14,0)]
	[Native]
	public enum AEAssessmentErrorCode : long {
		Unknown = 1
	}

	[iOS (14, 0)]
	[MacCatalyst (14,0)]
	[Native]
	enum AEAutocorrectMode : long {
		None = 0,
		Spelling = 1 << 0,
		Punctuation = 1 << 1,
	}
	
	[Mac (10,15,4), iOS (13,4)]
	[MacCatalyst (14,0)]
	[BaseType (typeof (NSObject))]
	interface AEAssessmentConfiguration : NSCopying {

		[NoMac, iOS (14, 0)]
		[Export ("autocorrectMode")]
		AEAutocorrectMode AutocorrectMode { get; set; }

		[NoMac, iOS (14, 0)]
		[Export ("allowsSpellCheck")]
		bool AllowsSpellCheck { get; set; }

		[NoMac, iOS (14, 0)]
		[Export ("allowsPredictiveKeyboard")]
		bool AllowsPredictiveKeyboard { get; set; }

		[NoMac, iOS (14, 0)]
		[Export ("allowsKeyboardShortcuts")]
		bool AllowsKeyboardShortcuts { get; set; }

		[NoMac, iOS (14, 0)]
		[Export ("allowsActivityContinuation")]
		bool AllowsActivityContinuation { get; set; }

		[NoMac, iOS (14, 0)]
		[Export ("allowsDictation")]
		bool AllowsDictation { get; set; }

		[NoMac, iOS (14, 0)]
		[Export ("allowsAccessibilitySpeech")]
		bool AllowsAccessibilitySpeech { get; set; }

		[NoMac, iOS (14, 0)]
		[Export ("allowsPasswordAutoFill")]
		bool AllowsPasswordAutoFill { get; set; }

		[NoMac, iOS (14, 0)]
		[Export ("allowsContinuousPathKeyboard")]
		bool AllowsContinuousPathKeyboard { get; set; }
	}

	[Mac (10,15,4), iOS (13,4)]
	[MacCatalyst (14,0)]
	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor]
	interface AEAssessmentSession {

		[Wrap ("WeakDelegate")]
		[NullAllowed]
		IAEAssessmentSessionDelegate Delegate { get; set; }

		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		[Export ("active")]
		bool Active { [Bind ("isActive")] get; }

		[Export ("initWithConfiguration:")]
		IntPtr Constructor (AEAssessmentConfiguration configuration);

		[Export ("begin")]
		void Begin ();

		[Export ("end")]
		void End ();
	}

	interface IAEAssessmentSessionDelegate { }

	[Mac (10,15,4), iOS (13,4)]
	[MacCatalyst (14,0)]
	[Protocol, Model (AutoGeneratedName = true)]
	[BaseType (typeof (NSObject))]
	interface AEAssessmentSessionDelegate {

		[Export ("assessmentSessionDidBegin:")]
		void DidBegin (AEAssessmentSession session);

		[Export ("assessmentSession:failedToBeginWithError:")]
		void FailedToBegin (AEAssessmentSession session, NSError error);

		[Export ("assessmentSession:wasInterruptedWithError:")]
		void WasInterrupted (AEAssessmentSession session, NSError error);

		[Export ("assessmentSessionDidEnd:")]
		void DidEnd (AEAssessmentSession session);
	}
}
