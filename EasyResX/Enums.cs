
namespace EasyResX
{
    // operation modes for processing
    public enum ResXOperationModes
    {
        Unknown = 0,

        // synchronizes all cultures resx with the default one
        Synchronize = 1,

        // creates package
        CreatePackage = 2,

        // gets available cultures in project folder
        GetAvailableCultures = 4,

        // creates new culture
        CreateNewCulture = 5
    }    
}