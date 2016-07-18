using Xunit;

namespace NuGet.Packaging.VisualStudio.UnitTests
{
	public class PlatformTemplateSpec
	{
		[Theory]
		[InlineData("", "")]
		[InlineData(null, null)]
		[InlineData("", null)]
		[InlineData(null, "")]
		public void when_name_or_value_are_null_empty_then_try_parse_returns_false(
			string nullOrEmptyName, string nullOrEmptyValue)
		{
			var parameter = default(PlatformTemplate);

			Assert.False(PlatformTemplate.TryParse(
				nullOrEmptyName, nullOrEmptyValue, out parameter));
		}


		[Theory]
		[InlineData("$PlatformTemplate.iOS$", "Xamarin.iOS.Library|Xamarin.iOS", "iOS", "Xamarin.iOS.Library", "Xamarin.iOS")]
		[InlineData("$PlatformTemplate.Android$", "Xamarin.Android.Library|Xamarin.Android", "Android", "Xamarin.Android.Library", "Xamarin.Android")]
		public void when_name_and_value_are_valid_then_parameter_is_parsed(
			string name,
			string value,
			string expectedSuffix,
			string expectedTemplateId,
			string expectedDisplayName)
		{
			var parameter = default(PlatformTemplate);

			Assert.True(PlatformTemplate.TryParse(
				name, value, out parameter));

			Assert.Equal(expectedSuffix, parameter.Suffix);
			Assert.Equal(expectedTemplateId, parameter.TemplateId);
			Assert.Equal(expectedDisplayName, parameter.DisplayName);
		}
	}
}