namespace Serenity.CodeGenerator;

public partial class TSTypeListerASTTests
{
    [Fact]
    public void Resolves_Exported_Editor_Types_From_Serenity_Module()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.CreateDirectory(root);
        string myDialog = root + "Modules/Test/MyDialog.ts";
        fileSystem.CreateDirectory(fileSystem.GetDirectoryName(myDialog));
        fileSystem.WriteAllText(myDialog, @"
import { EntityDialog } from '@serenity-is/corelib';

export class MyDialog extends EntityDialog {
}
");
        string corelibPackage = root + "node_modules/@serenity-is/corelib/package.json";
        string corelibIndexTS = root + "node_modules/@serenity-is/corelib/src/index.ts";
        string serenityIndexTS = root + "node_modules/@serenity-is/corelib/src/serenity/index.ts";
        string editorTypesTS = root + "node_modules/@serenity-is/corelib/src/ui/editors/editortypes.ts";
        fileSystem.CreateDirectory(fileSystem.GetDirectoryName(serenityIndexTS));
        fileSystem.CreateDirectory(fileSystem.GetDirectoryName(editorTypesTS));

        fileSystem.WriteAllText(corelibPackage, @"{
    ""name"": ""@serenity-is/corelib"",
    ""types"": ""src/index.ts"",
    ""typesVersions"": {
        ""*"": {
            ""serenity"": [ ""src/serenity/index.ts"" ]
        }
    }
}");
        fileSystem.WriteAllText(corelibIndexTS, @"
export * from ""./serenity"";
");

        fileSystem.WriteAllText(serenityIndexTS, @"
export * from ""../ui/editors/editortypes"";
");

        fileSystem.WriteAllText(editorTypesTS, @"
import { Decorators } from ""../../decorators"";
import { IStringValue } from ""../../interfaces"";
import { Widget } from ""../widgets/widget"";

@Decorators.registerEditor('Serenity.StringEditor', [IStringValue])
export class StringEditor extends Widget<any> {
}

@Decorators.registerEditor('Serenity.PasswordEditor')
export class PasswordEditor extends StringEditor {
}
");

        var tl = new TSTypeListerAST(fileSystem, tsConfigDir: root, tsConfig: new TSConfig
        {
            CompilerOptions = new TSConfig.CompilerConfig
            {
                Module = "ES2015"
            }
        });
        tl.AddInputFile(myDialog);

        var types = tl.ExtractTypes();

        var stringEditor = Assert.Single(types, x => x.FullName == "@serenity-is/corelib:StringEditor");
        var attr = Assert.Single(stringEditor.Attributes);
        Assert.Equal("@serenity-is/corelib:Decorators.registerEditor", attr.Type);
        Assert.Collection(attr.Arguments, arg1 =>
        {
            Assert.Equal("Serenity.StringEditor", arg1.Value);
        }, arg2 => { });

        var passwordEditor = Assert.Single(types, x => x.FullName == "@serenity-is/corelib:PasswordEditor");
        attr = Assert.Single(passwordEditor.Attributes);
        Assert.Equal("@serenity-is/corelib:Decorators.registerEditor", attr.Type);
        Assert.Equal("Serenity.PasswordEditor", Assert.Single(attr.Arguments).Value);
    }
}
