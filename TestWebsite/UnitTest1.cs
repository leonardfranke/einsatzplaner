using System.Diagnostics;
using System.Net.Http.Json;

namespace TestWebsite
{
    [TestFixture]
    public class Tests : PageTest
    {
        private Process? _firebaseEmulator;

        [OneTimeSetUp]
        public async Task StartFirebaseEmulator()
        {
            _firebaseEmulator = Process.Start(new ProcessStartInfo
            { 
                WorkingDirectory = "emulator_firebase",
                FileName = "firebase",
                Arguments = "emulators:start --project emulator --import .\\Export\\",
                UseShellExecute = true,
                CreateNoWindow = true
            });
            
            int i = 0;
            while(i < 60)
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("http://localhost:4400/emulators");
                var emulatorList = await response.Content.ReadFromJsonAsync<EmulatorsList>();
                if (emulatorList?.firestore != null && emulatorList?.auth != null)
                    return;
                Thread.Sleep(1000);
                i++;
            }
            throw new InvalidOperationException("Firebase Emulators could not be detected");
        }

        [Test]
        public async Task TestFirstRegistration()
        {
            await Page.GotoAsync("https://localhost:7144/dachse-badlaer");
            await Expect(Page).ToHaveURLAsync(new Regex(".*/login"));
            await Page.GetByTestId("registrieren-tab").ClickAsync();

            await Page.GetByTestId("registrieren-firstname").FillAsync("Leonard");
            await Page.GetByTestId("registrieren-lastname").FillAsync("Franke");
            await Page.GetByTestId("registrieren-email").FillAsync("leonard.franke@example.com");
            await Page.GetByTestId("registrieren-password").FillAsync("passwort");
            await Page.GetByTestId("registrieren-send").ClickAsync();

            await Page.GetByText("E-Mail anfordern").ClickAsync();

            var httpClient = new HttpClient();
            var response = await httpClient.GetFromJsonAsync<EmailVerificationListResponse>("http://localhost:9099/emulator/v1/projects/emulator/oobCodes");
            var verification = response.oobCodes.Find(item => item.email == "leonard.franke@example.com" && item.requestType == "VERIFY_EMAIL");
            Assert.NotNull(verification);
            await httpClient.GetAsync(verification.oobLink);

            await Page.GotoAsync("https://localhost:7144/dachse-badlaer");
            await Page.GetByText("Anfrage senden").ClickAsync();
            await Page.GotoAsync("https://localhost:7144/dachse-badlaer");

            var createEventButton = Page.GetByText("Event erstellen");
            await Expect(createEventButton).ToBeVisibleAsync();
        }

        [OneTimeTearDown]
        public void StopFirebaseEmulator()
        {
            _firebaseEmulator?.Kill(true);
        }
    }

    public class EmulatorsList
    {
        public Emulator firestore { get; set; }
        public Emulator auth { get; set; }
    }

    public class Emulator
    {
        public string name { get; set; }
    }

    public class EmailVerificationListResponse
    {
        public List<EmailVerificationResponse> oobCodes { get; set; }
    }

    public class EmailVerificationResponse
    {
        public string email { get; set; }
        public string requestType { get; set; }
        public string oobLink { get; set; }
    }
}
