using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

public static class FirebaseInitializer
{
    private static bool isInitialized = false;
    public static void Initialize()
    {
        if (!isInitialized)
        {
            try {
                string credentialsPath = "../../../Properties/noco2-e46fa-firebase-adminsdk-fta0k-c6f39e61a6.json";

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                });

                isInitialized = true;
            } catch (ArgumentException) {
                Console.WriteLine("Firebase Initialized");
            }

        }
    }
}