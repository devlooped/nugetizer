using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace NuGet.Packaging.VisualStudio
{
	abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
	{
		readonly Dictionary<string, List<ValidationResult>> errorSummaryByColumnName = new Dictionary<string, List<ValidationResult>>();

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual string Error { get; }

		public virtual bool IsValid { get; set; }

		public virtual string this[string columnName]
		{
			get
			{
				string result = null;

				var propertyInfo = GetType().GetTypeInfo().GetProperty(columnName);
				if (propertyInfo != null)
				{
					var value = propertyInfo.GetValue(this);
					var validationContext = new ValidationContext(this)
					{
						MemberName = columnName
					};

					var validationResults = new List<ValidationResult>();
					if (!Validator.TryValidateProperty(value, validationContext, validationResults))
						result = string.Join(Environment.NewLine, validationResults.Select(x => x.ErrorMessage));

					errorSummaryByColumnName[columnName] = validationResults;

					IsValid = !errorSummaryByColumnName.Any(x => x.Value.Any());
				}

				return result;
			}
		}
	}
}
