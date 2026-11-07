using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TestBench.Attributes;

public class CheckboxRequiredAttribute : ValidationAttribute, IClientModelValidator {

    public override bool IsValid(object? value)
        => value is bool ? (bool)value : false;

    public void AddValidation(ClientModelValidationContext context) {
        context.Attributes.Add("data-val-checkboxrequired", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
    }
}
