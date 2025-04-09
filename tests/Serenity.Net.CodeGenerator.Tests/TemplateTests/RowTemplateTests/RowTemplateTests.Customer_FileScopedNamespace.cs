namespace Serenity.CodeGenerator;

public partial class RowTemplateTests
{
    [Fact]
    public void Customer_FileScopedNamespace()
    {
        var model = new CustomerEntityModel
        {
            FileScopedNamespaces = true
        };
        var actual = RenderTemplate(model);

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        var expected = Regex.Replace(Customer_Expected_Defaults,
            @"^namespace TestNamespace\.TestModule",
            "namespace TestNamespace.TestModule;\n",
            RegexOptions.Multiline);

        expected = Regex.Replace(expected,
            @"^([{}]\r?\n?|    )", "", RegexOptions.Multiline).TrimEnd();
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

        AssertEqual(expected, actual);
        Assert.Contains("namespace TestNamespace.TestModule;", actual);
    }
}
