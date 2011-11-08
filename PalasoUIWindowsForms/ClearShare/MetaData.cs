﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Palaso.CommandLineProcessing;
using Palaso.Extensions;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace Palaso.UI.WindowsForms.ClearShare
{
	/// <summary>
	/// Provides reading and writing of metdata, currently for any file which exiftool can read AND write (images, pdf).
	/// ExifTool can read many more formats that it can write (video, html, docx), but I have not tested those yet  (should be easy).
	/// ExifTool can also read/write sidecar files, but that is not yet implemented here, either (should be easy).
	/// Where multiple metadata formats are in a file (XMP, EXIF, IPTC-IIM), exif provides conformance to the MedatData
	/// Working Group guidelines: http://www.metadataworkinggroup.org/pdf/mwg_guidance.pdf, which we use by telling exiftool to
	/// to "-use MWG": http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/MWG.html.  E.g., this puts the "Copyright" into both the exif "copyright", and "xmp:Rights".
	///
	/// Microsoft Pro Photo Tools: http://www.microsoft.com/download/en/details.aspx?id=13518
	/// </summary>
	public class Metadata
	{
		public Metadata()
		{
			IsEmpty = true;
		}

		/// <summary>
		/// Create a MetadataAccess by reading an existing media file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Metadata FromFile(string path)
		{
			var m = new Metadata();
			m._path = path;

			var properties = GetImageProperites(path);

			foreach (var assignment in MetadataAssignments)
			{
				string propertyValue;
				if (properties.TryGetValue(assignment.ResultLabel.ToLower(), out propertyValue))
				{
					assignment.AssignmentAction.Invoke(m, propertyValue);
					m.IsEmpty = false;

				}
			}
			m.License = LicenseInfo.FromXmp(properties);

			//clear out the change-setting we just caused, because as of right now, we are clean with respect to what is on disk, no need to save.
			m.HasChanges = false;
			return m;
		}

		private LicenseInfo _license;

		///<summary>
		/// 0 or more licenses offered by the copyright holder
		///</summary>
		public LicenseInfo License
		{
			get { return _license; }
			set
			{
				if (value != _license)
					HasChanges = true;
				_license = value;
				if (!(value is NullLicense))
					IsEmpty = false;
			}
		}

		private string _copyrightNotice;
		public string CopyrightNotice
		{
			get { return _copyrightNotice; }
			set
			{
				if (value!=null && value.Trim().Length == 0)
					value = null;
				if (value != _copyrightNotice)
					HasChanges = true;
				_copyrightNotice = value;
				if (!String.IsNullOrEmpty(_copyrightNotice))
					IsEmpty = false;
			}
		}

		private string _creator;

		/// <summary>
		/// Use this for artist, photographer, company, whatever.  It is mapped to EXIF:Author and XMP:AttributionName
		/// </summary>
		public string Creator
		{
			get { return _creator; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _creator)
					HasChanges = true;
				_creator = value;
				if (!String.IsNullOrEmpty(_creator))
					IsEmpty = false;
			}
		}

		private string _attributionUrl;

		/// <summary>
		/// Use this for the site to link to in attribution.  It is mapped to XMP-Creative Commons--AttributionUrl, but may be used even if you don't have a creative commons license
		/// </summary>
		public string AttributionUrl
		{
			get { return _attributionUrl; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _attributionUrl)
					HasChanges = true;
				_attributionUrl = value;
				if (!String.IsNullOrEmpty(_attributionUrl))
					IsEmpty = false;
			}
		}

		private string _collectionName;

		/// <summary>
		/// E.g. "Art of Reading"
		/// </summary>
		public string CollectionName
		{
			get { return _collectionName; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _collectionName)
					HasChanges = true;
				_collectionName = value;
				if (!String.IsNullOrEmpty(_collectionName))
					IsEmpty = false;

			}
		}

		private string _collectionUri;
		public string CollectionUri
		{
			get { return _collectionUri; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _collectionUri)
					HasChanges = true;
				_collectionUri = value;
				if (!String.IsNullOrEmpty(_collectionUri))
					IsEmpty = false;

			}
		}

		/// <summary>
		/// Used during UI editing to know if we should enable the OK button
		/// </summary>
		public bool IsMinimallyComplete
		{
			get
			{
				//I'm thinking, license is secondary. Primary is who did it or what the copyright is... if you have a license without any of that... it's kind of bogus.
				return !String.IsNullOrEmpty(CopyrightNotice) || !String.IsNullOrEmpty(Creator) ||
					   !String.IsNullOrEmpty(CollectionUri);
			}
		}

		private static Dictionary<string, string> GetImageProperites(string path)
		{
			var exifPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			var args = new StringBuilder();
			foreach (var assignment in MetadataAssignments)
			{
				args.Append(" " + assignment.Switch + " ");
			}
			var result = CommandLineRunner.Run(exifPath, String.Format("{0} \"{1}\"", args.ToString(), path), Path.GetDirectoryName(path), 5, new NullProgress());
#if DEBUG
			Debug.WriteLine("reading");
			Debug.WriteLine(args.ToString());
			Debug.WriteLine(result.StandardError);
			Debug.WriteLine(result.StandardOutput);
#endif
			var lines = result.StandardOutput.SplitTrimmed('\n');
			var values = new Dictionary<string, string>();
			foreach (var line in lines)
			{
				var parts = line.SplitTrimmed(':');
				if (parts.Count < 2)
					continue;

				//recombine any parts of the value which had a colon (like a url does)
				string value = parts[1];
				for (int i = 2; i < parts.Count; ++i)
					value = value + ":" + parts[i];

				values.Add(parts[0].ToLower(), value);
			}
			return values;
		}


		private class MetadataAssignement
		{
			public Func<Metadata, string> GetStringFunction { get; set; }
			public Func<Metadata, bool> ShouldSetValue { get; set; }
			public string Switch;
			public string ResultLabel;
			public Action<Metadata, string> AssignmentAction;

			public MetadataAssignement(string Switch, string resultLabel, Action<Metadata, string> assignmentAction, Func<Metadata, string> stringProvider)
				: this(Switch, resultLabel, assignmentAction, stringProvider, p => !String.IsNullOrEmpty(stringProvider(p)))
			{
			}

			public MetadataAssignement(string @switch, string resultLabel, Action<Metadata, string> assignmentAction, Func<Metadata, string> stringProvider, Func<Metadata, bool> shouldSetValueFunction)
			{
				GetStringFunction = stringProvider;
				ShouldSetValue = shouldSetValueFunction;
				Switch = @switch;
				ResultLabel = resultLabel;
				AssignmentAction = assignmentAction;
			}
		}

		private static List<MetadataAssignement> MetadataAssignments
		{
			get
			{
				var assignments = new List<MetadataAssignement>();
				assignments.Add(new MetadataAssignement("-copyright", "copyright", (p, value) => p.CopyrightNotice = value, p => p.CopyrightNotice));

				assignments.Add(new MetadataAssignement("-Author", "Author", (p, value) => p.Creator = value, p => p.Creator));
				assignments.Add(new MetadataAssignement("-XMP:CollectionURI", "Collection URI", (p, value) => p.CollectionUri = value, p => p.CollectionUri));
				assignments.Add(new MetadataAssignement("-XMP:CollectionName", "Collection Name", (p, value) => p.CollectionName = value, p => p.CollectionName));
				assignments.Add(new MetadataAssignement("-XMP-cc:AttributionURL", "Attribution URL", (p, value) => p.AttributionUrl = value, p => p.AttributionUrl));
				assignments.Add(new MetadataAssignement("-XMP-cc:License", "license",
													   (p, value) => { },//p.License=LicenseInfo.FromUrl(value), //we need to use for all the properties to set up the license
													   p => p.License.Url, p => p.License !=null));
				return assignments;
			}
		}

		public bool IsEmpty { get; private set; }

		private bool _hasChanges;
		public bool HasChanges
		{
			get
			{
				return _hasChanges || (License!=null && License.HasChanges);
			}
			set
			{
				_hasChanges = value;
				if(!value && License!=null)
					License.HasChanges = false;
			}
		}

		/// <summary>
		/// Attempt to cut off notices like "Copyright Blah 2009. Blah blah blah"
		/// </summary>
		public string ShortCopyrightNotice
		{
			get {
				var i = CopyrightNotice.IndexOf('.');
				if(i<0)
					return CopyrightNotice;
				return CopyrightNotice.Substring(0, i);
			}
		}

		private string _path;

		public void Write()
		{
			Write(_path);
		}

		public void Write(string path)
		{
			var exifToolPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			//-E   -overwrite_original_in_place -d %Y
			StringBuilder arguments = new StringBuilder();

			foreach (var assignment in MetadataAssignments)
			{
				if (assignment.ShouldSetValue(this))
				{
					arguments.AppendFormat(" " + assignment.Switch + "=\"" + assignment.GetStringFunction(this) + "\" ");
				}
			}

			if (arguments.ToString().Length == 0)
			{
				//no metadata
				return;
			}

			//NB: when it comes time to having multiple contibutors, see Hatton's question on http://u88.n24.queensu.ca/exiftool/forum/index.php/topic,3680.0.html.  We need -sep ";" or whatever to ensure we get a list.

			arguments.AppendFormat(" -use MWG ");  //see http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/MWG.html  and http://www.metadataworkinggroup.org/pdf/mwg_guidance.pdf
			arguments.AppendFormat(" \"{0}\"", path);
			var result = CommandLineRunner.Run(exifToolPath, arguments.ToString(), Path.GetDirectoryName(_path), 5, new NullProgress());
			// -XMP-dc:Rights="Copyright SIL International" -XMP-xmpRights:Marked="True" -XMP-cc:License="http://creativecommons.org/licenses/by-sa/2.0/" *.png");
#if DEBUG
			Debug.WriteLine("writing");
			Debug.WriteLine(arguments.ToString());
			Debug.WriteLine(result.StandardError);
			Debug.WriteLine(result.StandardOutput);
#endif
			//as of right now, we are clean with respect to what is on disk, no need to save.
			HasChanges = false;
		}

		public void SetupReasonableLicenseDefaultBeforeEditing()
		{
			if(License == null || License is NullLicense)
			{
				License= new CreativeCommonsLicense(true,true,CreativeCommonsLicense.DerivativeRules.Derivatives);
			}
		}

		public Metadata DeepCopy()
		{
			return (Metadata) CloneObject(this);
		}

		/// <summary>
		/// This is actually a generic cloning method, copied into this class just because UI needs to clone this class.
		/// From http://stackoverflow.com/questions/78536/cloning-objects-in-c-sharp/78551#78551
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private static object CloneObject(object source)
		{
			//grab the type and create a new instance of that type
			Type sourceType = source.GetType();
			//  var target =  Activator.CreateInstance(source.GetType(), null);
			object target = Activator.CreateInstance(sourceType, true);

			//grab the properties
			PropertyInfo[] PropertyInfo =
				sourceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			//iterate over the properties and if it has a 'set' method assign it from the source TO the target
			foreach (PropertyInfo item in PropertyInfo)
			{
				if (item.CanWrite)
				{
					//value types can simply be 'set'
					if (item.PropertyType.IsValueType || item.PropertyType.IsEnum ||
						item.PropertyType.Equals(typeof(System.String)))
					{
						item.SetValue(target, item.GetValue(source, null), null);
					}
						//object/complex types need to recursively call this method until the end of the tree is reached
					else
					{
						object propertyValue = item.GetValue(source, null);
						if (propertyValue == null)
						{
							item.SetValue(target, null, null);
						}
						else
						{
							Type z = item.PropertyType;
							item.SetValue(target, CloneObject(propertyValue), null);
							//item.SetValue(target, CloneObject<LicenseInfo>(propertyValue as LicenseInfo), null);
						}
					}
				}
			}
			//return the new item
			return target;
		}
	}
}