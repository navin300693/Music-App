namespace MusicApp.Common.Attributes;

public class AllowedExtensionsAttribute(string[] extensions) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // If the value is null, we let the [Required] attribute handle the error
        if (value is null) return ValidationResult.Success;

        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!extensions.Contains(extension))
            {
                return new ValidationResult(GetErrorMessage(extension));
            }
        }

        return ValidationResult.Success;
    }

    private string GetErrorMessage(string extension)
        => $"File extension {extension} is not allowed. Allowed extensions are: {string.Join(", ", extensions)}";
}