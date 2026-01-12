using Microsoft.Playwright;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;

class PlaywrightTestonIPad
{
    public static async Task main(string[] args)
    {
        using var playwright = await Playwright.CreateAsync();

        string user, accessKey;
        user = Environment.GetEnvironmentVariable("LT_USERNAME");
        accessKey = Environment.GetEnvironmentVariable("LT_ACCESS_KEY");

        Dictionary<string, object> capabilities = new Dictionary<string, object>();
        Dictionary<string, string> ltOptions = new Dictionary<string, string>();

        ltOptions.Add("name", "Playwright Login Test on iPad");
        ltOptions.Add("build", "Playwright C-Sharp tests on Hyperexecute");
        ltOptions.Add("platform", Environment.GetEnvironmentVariable("HYPEREXECUTE_PLATFORM"));
        ltOptions.Add("user", user);
        ltOptions.Add("accessKey", accessKey);

        capabilities.Add("browserName", "Chrome");
        capabilities.Add("browserVersion", "latest");
        capabilities.Add("LT:Options", ltOptions);

        string capabilitiesJson = JsonConvert.SerializeObject(capabilities);

        string cdpUrl = "wss://cdp.lambdatest.com/playwright?capabilities=" + Uri.EscapeDataString(capabilitiesJson);

        await using var browser = await playwright.Chromium.ConnectAsync(cdpUrl);

        // Documentation: https://playwright.dev/docs/emulation#devices
        // Supported devices: https://github.com/microsoft/playwright/blob/main/packages/playwright-core/src/server/deviceDescriptorsSource.json
        var context = await browser.NewContextAsync(playwright.Devices["iPad Pro 11 landscape"]);
        var page = await context.NewPageAsync();

        try {
            // Navigate to the login page
            await page.GotoAsync("https://the-internet.herokuapp.com/login");
            
            // Fill in the username
            await page.FillAsync("#username", "tomsmith");
            
            // Fill in the password
            await page.FillAsync("#password", "SuperSecretPassword!");
            
            // Click the Login button
            await page.ClickAsync("button[type='submit']");
            
            // Wait for navigation and check for success
            await page.WaitForSelectorAsync(".flash.success");
            
            var successMessage = await page.Locator(".flash.success").TextContentAsync();

            if (successMessage != null && successMessage.Contains("You logged into a secure area!"))
            {
                // Use the following code to mark the test status.
                await SetTestStatus("passed", "Login successful on iPad", page);
            }
            else {
                await SetTestStatus("failed", "Login failed - success message not found", page);
            }
        }
        catch (Exception err) {
            await SetTestStatus("failed", err.Message, page);
        }
        await browser.CloseAsync();
    }

    public static async Task SetTestStatus(string status, string remark, IPage page) {
        await page.EvaluateAsync("_ => {}", "lambdatest_action: {\"action\": \"setTestStatus\", \"arguments\": {\"status\":\"" + status + "\", \"remark\": \"" + remark + "\"}}");
    }
}
