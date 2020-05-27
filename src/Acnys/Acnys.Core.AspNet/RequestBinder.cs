using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json;

namespace Acnys.Core.AspNet
{
    public static class RequestBinderExtensions
    {
        public static MvcOptions UseRequestBinder(this MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, new RequestBinderProvider());
            return options;
        }
    }

    public static class RequestBinderHelper
    {
        public static bool IsRequest(this Type type) => typeof(ICommand).IsAssignableFrom(type) || typeof(IQuery<>).IsAssignableFrom(type);
    }

    public class RequestBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (!bindingContext.ModelType.IsRequest()) bindingContext.Result = ModelBindingResult.Failed();

            using var reader = new StreamReader(bindingContext.HttpContext.Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            bindingContext.Result = ModelBindingResult.Success(JsonConvert.DeserializeObject(json, bindingContext.ModelType));
        }
    }

    public class RequestBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Metadata.ModelType.IsRequest() ? new BinderTypeModelBinder(typeof(RequestBinder)) : null;
        }
    }
}
