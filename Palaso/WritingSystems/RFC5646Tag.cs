﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.WritingSystems
{
	public class RFC5646Tag
	{
		private string _language;
		private string _script;
		private string _region;
		private string _variant;

		public RFC5646Tag(string language, string script, string region, string variant)
		{
			_language = language;
			_script = script;
			_region = region;
			_variant = variant;
		}

		public string Language
		{
			get { return _language; }
			set { _language = value; }
		}

		public string Script
		{
			get { return _script; }
			set { _script = value; }
		}

		public string Region
		{
			get { return _region; }
			set { _region = value; }
		}

		public string Variant
		{
			get { return _variant; }
			set { _variant = value; }
		}

		//This method defines what is currently considered a valid RFC 5646 language tag by palaso.
		//At the moment this is almost anything.
		public static bool IsValid(RFC5646Tag tagToCheck)
		{
			if (tagToCheck.Language.Contains("x-audio")) { return false; }
			if (tagToCheck.Variant == "x-audio" && tagToCheck.Script != "Zxxx") { return false; }
			return true;
		}

		public static RFC5646Tag GetValidTag(RFC5646Tag tagToConvert)
		{
			if (IsValid(tagToConvert)) { return tagToConvert; }

			RFC5646Tag validRfc5646Tag = null;

			if (tagToConvert.Language.Contains("x-audio"))
			{
				string newLanguageTag = tagToConvert.Language.Split('-')[0];
				validRfc5646Tag = RFC5646TagForVoiceWritingSystem(newLanguageTag);
			}
			if (tagToConvert.Variant == "x-audio" && tagToConvert.Script != "Zxxx")
			{
				string newLanguageTag = tagToConvert.Language.Split('-')[0];
				validRfc5646Tag = RFC5646TagForVoiceWritingSystem(newLanguageTag);
			}
			if (!IsValid(validRfc5646Tag))
			{
				throw new InvalidOperationException("The palaso library is not able to covert this tag to a valid form.");
			}
			return validRfc5646Tag;
		}

		public static RFC5646Tag RFC5646TagForVoiceWritingSystem(string language)
		{
			return new RFC5646Tag(language, "Zxxx", "", "x-audio");
		}

		public static bool IsRFC5646TagForVoiceWritingSystem(RFC5646Tag rfcTag)
		{
			if(rfcTag.Script == "Zxxx" && rfcTag.Variant == "x-audio")
			{
				return true;
			}
			return false;
		}
	}
}