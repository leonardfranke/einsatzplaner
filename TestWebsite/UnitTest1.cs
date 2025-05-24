using System.Net.Http.Json;
using Microsoft.Playwright;

namespace TestWebsite
{
    [TestFixture]
    public class Tests : PageTest
    {
        public override BrowserNewContextOptions ContextOptions()
        {
            var options = base.ContextOptions() ?? new BrowserNewContextOptions();
            options.IgnoreHTTPSErrors = true;
            return options;
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

            await Page.GotoAsync("https://localhost:7144/dachse-badlaer/groups");

            var createGroupButton = Page.GetByText("Gruppe hinzufügen", new PageGetByTextOptions { Exact = true});
            await Expect(createGroupButton).ToBeVisibleAsync();
            await createGroupButton.ClickAsync();
            await Page.GetByTestId("group-name").FillAsync("Herren");
            await Page.GetByText("Speichern").ClickAsync();
            var groupText = Page.GetByText("Herren - Ohne Mitglieder");
            await Expect(groupText).ToBeVisibleAsync();

            var createRoleButton = Page.GetByText("Rolle hinzufügen");
            await Expect(createRoleButton).ToBeVisibleAsync();
            await createRoleButton.ClickAsync();
            await Page.GetByLabel("Name").FillAsync("Anschreiber");
            await Page.GetByLabel("Sperrzeitraum").FillAsync("20");
            await Page.GetByText("Speichern").ClickAsync();
            var roleText = Page.GetByText("Anschreiber - Ohne Mitglieder");
            await Expect(roleText).ToBeVisibleAsync();
            var roleInfoText = Page.GetByText("Sperrzeitraum: 20 Tage");
            await Expect(roleInfoText).ToBeVisibleAsync();

            var createRequirementGroupButton = Page.GetByText("Bedarfsgruppe hinzufügen");
            await Expect(createRequirementGroupButton).ToBeVisibleAsync();
            await createRequirementGroupButton.ClickAsync();
            await Page.GetByText("Hinzufügen", new PageGetByTextOptions { Exact = true}).ClickAsync();
            await Page.GetByText("Anschreiber", new PageGetByTextOptions { Exact = true }).ClickAsync();
            await Page.GetByTestId("amount-Anschreiber").FillAsync("3");
            await Page.GetByText("Speichern").ClickAsync();            
            var anschreiberInfoText = Page.GetByText("3x Anschreiber");
            await Expect(anschreiberInfoText).ToBeVisibleAsync();
            
        }
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
