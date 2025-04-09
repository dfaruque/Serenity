using static Serenity.CodeGenerator.CustomerEntityInputs;

namespace Serenity.CodeGenerator;

public partial class EntityModelFactoryTests
{
    [Fact]
    public void Throws_ArgumentNull_If_Inputs_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new EntityModelFactory().Create(inputs: null));
    }

    private string AttrName<TAttr>()
    {
        var fullName = typeof(TAttr).FullName;
        if (fullName.EndsWith("Attribute"))
            return fullName[0..^9];

        return fullName;
    }

    [Fact]
    public void Customer_Defaults()
    {
        var generator = new EntityModelFactory();
        var inputs = new CustomerEntityInputs();
        var model = generator.Create(inputs);
        Assert.Equal(Customer, model.ClassName);
        Assert.Equal(TestConnection, model.ConnectionKey);
        Assert.Equal(TestModule, model.Module);
        Assert.Equal(TestPermission, model.Permission);
        Assert.Equal(TestNamespace, model.RootNamespace);
        Assert.Equal(Customer + "Row", model.RowClassName);
        Assert.Equal(TestSchema, model.Schema);
        Assert.Equal(Customer, model.Tablename);
        Assert.Equal(Customer, model.Title);
        Assert.Equal(CustomerId, model.IdField);
        Assert.Equal(typeof(Row<>).FullName.Split('`')[0] + $"<{Customer}Row.RowFields>", model.RowBaseClass);
        Assert.Equal(CustomerName, model.NameField);
        Assert.Equal("", model.FieldPrefix);
        Assert.True(model.AspNetCore);
        Assert.True(model.NET5Plus);
        Assert.False(model.DeclareJoinConstants);

        Assert.Collection(model.Fields,
            customerId =>
            {
                Assert.Equal(CustomerId, customerId.PropertyName);
                Assert.Equal("Int32", customerId.FieldType);
                Assert.Equal("int", customerId.DataType);
                Assert.Equal("number", customerId.TSType);
                Assert.Equal(CustomerId, customerId.Name);
                Assert.Equal("Customer Id", customerId.Title);
                Assert.True(customerId.IsValueType);
                Assert.True(customerId.OmitInForm);
                Assert.Null(customerId.Size);
                Assert.Equal(0, customerId.Scale);


                var identity = Assert.Single(customerId.FlagList);
                Assert.Equal(AttrName<IdentityAttribute>(), identity.TypeName);
                Assert.Empty(identity.Arguments);

                Assert.Collection(customerId.AttributeList,
                    displayName =>
                    {
                        Assert.Equal(AttrName<DisplayNameAttribute>(), displayName.TypeName);
                        Assert.Equal("Customer Id", Assert.Single(displayName.Arguments));
                    },
                    identity =>
                    {
                        Assert.Equal(AttrName<IdentityAttribute>(), identity.TypeName);
                        Assert.Empty(identity.Arguments);
                    },
                    idProperty =>
                    {
                        Assert.Equal(AttrName<IdPropertyAttribute>(), idProperty.TypeName);
                        Assert.Empty(idProperty.Arguments);
                    });

                Assert.Collection(customerId.ColAttributeList,
                    editLink =>
                    {
                        Assert.Equal(AttrName<EditLinkAttribute>(), editLink.TypeName);
                        Assert.Empty(editLink.Arguments);
                    },
                    displayName =>
                    {
                        Assert.Equal(AttrName<DisplayNameAttribute>(), displayName.TypeName);
                        Assert.Equal("Db.Shared.RecordId", Assert.Single(displayName.Arguments));
                    },
                    alignRight =>
                    {
                        Assert.Equal(AttrName<AlignRightAttribute>(), alignRight.TypeName);
                        Assert.Empty(alignRight.Arguments);
                    });
            },
            customerName =>
            {
                Assert.Equal(CustomerName, customerName.PropertyName);
                Assert.Equal("String", customerName.FieldType);
                Assert.Equal("string", customerName.DataType);
                Assert.Equal("string", customerName.TSType);
                Assert.Equal(CustomerName, customerName.Name);
                Assert.Equal("Customer Name", customerName.Title);
                Assert.False(customerName.IsValueType);
                Assert.False(customerName.OmitInForm);
                Assert.Equal(50, customerName.Size);


                var notNull = Assert.Single(customerName.FlagList);
                Assert.Equal(AttrName<NotNullAttribute>(), notNull.TypeName);
                Assert.Empty(notNull.Arguments);

                Assert.Collection(customerName.AttributeList,
                    displayName =>
                    {
                        Assert.Equal(AttrName<DisplayNameAttribute>(), displayName.TypeName);
                        Assert.Equal("Customer Name", Assert.Single(displayName.Arguments));
                    },
                    size =>
                    {
                        Assert.Equal(AttrName<SizeAttribute>(), size.TypeName);
                        Assert.Equal(50, Assert.Single(size.Arguments));
                    },
                    notNull =>
                    {
                        Assert.Equal(AttrName<NotNullAttribute>(), notNull.TypeName);
                        Assert.Empty(notNull.Arguments);
                    },
                    quickSearch =>
                    {
                        Assert.Equal(AttrName<QuickSearchAttribute>(), quickSearch.TypeName);
                        Assert.Empty(quickSearch.Arguments);
                    },
                    nameProperty =>
                    {
                        Assert.Equal(AttrName<NamePropertyAttribute>(), nameProperty.TypeName);
                        Assert.Empty(nameProperty.Arguments);
                    });

                var editLink = Assert.Single(customerName.ColAttributeList);
                Assert.Equal(AttrName<EditLinkAttribute>(), editLink.TypeName);
                Assert.Empty(editLink.Arguments);
            },
            cityId =>
            {
                Assert.Equal(CityId, cityId.PropertyName);
            });
    }

    [Fact]
    public void Customer_DeclareJoinConstants_False_Configuration()
    {
        var generator = new EntityModelFactory();
        var inputs = new CustomerEntityInputs();
        inputs.Config.DeclareJoinConstants = false;
        var model = generator.Create(inputs);
        Assert.False(model.DeclareJoinConstants);

        var joinAttr = model.Fields.FirstOrDefault(x => x.PropertyName == CityId)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == LeftJoinAttrName);
        Assert.NotNull(joinAttr);
        Assert.Equal($"{jCity}", Assert.Single(joinAttr.Arguments));

        var cityJoin = model.Joins.FirstOrDefault(x => x.Name == City);
        Assert.NotNull(cityJoin);

        var cityNameExpr = cityJoin.Fields.FirstOrDefault(x => x.PropertyName == CityName)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == ExpressionAttrName);
        Assert.NotNull(cityNameExpr);
        Assert.Equal($"{jCity}.[{CityName}]", Assert.Single(cityNameExpr.Arguments));

        var countryIdExpr = cityJoin.Fields.FirstOrDefault(x => x.PropertyName == CityCountryId)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == ExpressionAttrName);
        Assert.NotNull(countryIdExpr);
        Assert.Equal($"{jCity}.[{CountryId}]", Assert.Single(countryIdExpr.Arguments));
    }

    [Fact]
    public void Customer_DeclareJoinConstants_True_Configuration()
    {
        var generator = new EntityModelFactory();
        var inputs = new CustomerEntityInputs();
        inputs.Config.DeclareJoinConstants = true;
        var model = generator.Create(inputs);
        Assert.True(model.DeclareJoinConstants);

        var joinAttr = model.Fields.FirstOrDefault(x => x.PropertyName == CityId)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == LeftJoinAttrName);
        Assert.NotNull(joinAttr);
        Assert.Equal(jCity, Assert.IsType<RawCode>(Assert.Single(joinAttr.Arguments)).Code);

        var cityJoin = model.Joins.FirstOrDefault(x => x.Name == City);
        Assert.NotNull(cityJoin);

        var cityNameExpr = cityJoin.Fields.FirstOrDefault(x => x.PropertyName == CityName)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == ExpressionAttrName);
        Assert.NotNull(cityNameExpr);
        Assert.Equal("$\"{" + jCity + "}.[" + CityName + "]\"", Assert.IsType<RawCode>(Assert.Single(cityNameExpr.Arguments)).Code);

        var countryIdExpr = cityJoin.Fields.FirstOrDefault(x => x.PropertyName == CityCountryId)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == ExpressionAttrName);
        Assert.NotNull(countryIdExpr);
        Assert.Equal("$\"{" + jCity + "}.[" + CountryId + "]\"", Assert.IsType<RawCode>(Assert.Single(countryIdExpr.Arguments)).Code);
    }

    [Fact]
    public void Customer_ForeignFieldSelection_None_No_Include_No_Remove()
    {
        var generator = new EntityModelFactory();

        var inputs = new CustomerEntityInputs();
        inputs.Config.ForeignFieldSelection = GeneratorConfig.FieldSelection.None;
        inputs.Config.IncludeForeignFields = null;
        inputs.Config.RemoveForeignFields = null;

        var model = generator.Create(inputs);

        Assert.Empty(Assert.Single(model.Joins).Fields);
    }

    [Fact]
    public void Customer_ForeignFieldSelection_None_No_Include_Manual_Exclude()
    {
        var generator = new EntityModelFactory();

        var inputs = new CustomerEntityInputs();
        inputs.Config.ForeignFieldSelection = GeneratorConfig.FieldSelection.None;
        inputs.Config.IncludeForeignFields = null;
        inputs.Config.RemoveForeignFields = [CountryId];

        var model = generator.Create(inputs);

        Assert.Empty(Assert.Single(model.Joins).Fields);
    }

    [Fact]
    public void Customer_ForeignFieldSelection_None_Manual_Include()
    {
        var generator = new EntityModelFactory();

        var inputs = new CustomerEntityInputs();
        inputs.Config.ForeignFieldSelection = GeneratorConfig.FieldSelection.None;
        inputs.Config.IncludeForeignFields = [CountryId];
        inputs.Config.RemoveForeignFields = null;

        var model = generator.Create(inputs);

        Assert.Equal(CityCountryId, Assert.Single(Assert.Single(model.Joins).Fields).PropertyName);
    }

}
