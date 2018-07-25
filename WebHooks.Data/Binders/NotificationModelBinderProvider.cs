using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using WebHooks.Data.Models;

namespace WebHooks.Data.Binders
{
    public class NotificationModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Metadata.ModelType == typeof(Response<Notification>) ? new BinderTypeModelBinder(typeof(NotificationModelBinder)) : null;
        }
    }
}
