﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
using Palaso.Tests.Code;

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexSenseIClonableGenericTests:IClonableGenericTests<LexSense>
	{
		public override LexSense CreateNewClonable()
		{
			return new LexSense();
		}

		public override string ExceptionList
		{
			get { return ""; }
		}

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				var bindingList = new BindingList<LexExampleSentence>
									  {
										  new LexExampleSentence {TranslationType = "sentence1"},
										  new LexExampleSentence {TranslationType = "sentence2"}
									  };
				return new Dictionary<Type, object>
						   {
							   {typeof(BindingList<LexExampleSentence>), bindingList}
						   };
			}
		}
	}
}