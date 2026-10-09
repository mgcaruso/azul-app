using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Azul.Api.Common.Binding;

public class CamelCaseQueryNamesProvider : IBindingMetadataProvider
{
    public void CreateBindingMetadata(BindingMetadataProviderContext context)
    {
        var containerType = context.Key.ContainerType;
        if (context.Key.MetadataKind == ModelMetadataKind.Property
            && containerType?.Assembly == typeof(CamelCaseQueryNamesProvider).Assembly
            && !typeof(ControllerBase).IsAssignableFrom(containerType)
            && context.BindingMetadata.BinderModelName is null)
        {
            context.BindingMetadata.BinderModelName = JsonNamingPolicy.CamelCase.ConvertName(context.Key.Name!);
        }
    }
}
