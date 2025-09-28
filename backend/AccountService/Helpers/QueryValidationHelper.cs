namespace AccountService.Helpers;

public static class QueryValidationHelper
{
    public static bool TryValidateIntList(
        string? input,
        List<int> allowedValues,
        out List<int> result,
        out string? errorMessage)
    {
        result = new List<int>();
        errorMessage = null;

        if (string.IsNullOrEmpty(input))
            return true;

        try
        {
            result = input.Split(',')
                .Select(x => int.Parse(x.Trim()))
                .ToList();
        }
        catch (Exception e)
        {
            errorMessage = "Invalid integer format.";
            return false;
            
        }

        if (result.Any(x => !allowedValues.Contains(x)))
        {
            errorMessage = $"Allowed values: {string.Join(", ", allowedValues)}";
            return false;
        }
        
        return true;
    }

    public static bool TryValidateStringList(
        string? input,
        List<string> allowedValues,
        out List<string> result,
        out string? errorMessage)
    {
        result = new List<string>();
        errorMessage = null;
        
        if (string.IsNullOrEmpty(input))
            return true; 
        
        result = input.Split(',')
            .Select(x => x.Trim())
            .ToList();

        if (result.Any(x => !allowedValues.Contains(x)))
        {
            errorMessage = $"Allowed roles: {string.Join(", ", allowedValues)}";
            return false;
        }
        
        return true;
    }
}