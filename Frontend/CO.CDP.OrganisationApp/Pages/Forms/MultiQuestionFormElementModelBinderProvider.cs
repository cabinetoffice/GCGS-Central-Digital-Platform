using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class MultiQuestionFormElementModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.ModelType == typeof(FormElementMultiQuestionModel) ? new BinderTypeModelBinder(typeof(MultiQuestionFormElementModelBinder)) : null;
    }
}