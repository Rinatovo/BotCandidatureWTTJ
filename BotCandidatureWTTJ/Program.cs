using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class BotCandidatureSpontanee
{
    static void Main(string[] args)
    {
        IWebDriver driver = new ChromeDriver();
        driver.Manage().Window.Maximize();

        try
        {
            // Étape 1 : Accéder à la page Welcome to the Jungle
            driver.Navigate().GoToUrl("https://www.welcometothejungle.com/fr");
            Console.WriteLine("Page Welcome to the Jungle ouverte.");

            // Étape 2 : Connexion
            var loginButton = WaitForElement(driver, By.CssSelector("[data-testid='not-logged-visible-login-button']"), 10);
            loginButton.Click();

            var emailInput = WaitForElement(driver, By.Name("email_login"), 10);
            emailInput.SendKeys("votre mail");
            driver.FindElement(By.Name("password")).SendKeys("votre mot de passe*");

            var loginSubmitButton = driver.FindElement(By.CssSelector("[data-testid='login-button-submit']"));
            loginSubmitButton.Click();
            Thread.Sleep(5000);

            // Étape 3 : Recherche d'entreprises
            var findCompanyButton = WaitForElement(driver, By.CssSelector("[data-testid='menu-companies']"), 10);
            findCompanyButton.Click();

            var searchInput = WaitForElement(driver, By.CssSelector("[data-testid='companies-search-search-field-query']"), 10);
            searchInput.SendKeys("web");

            var locationInput = driver.FindElement(By.CssSelector("[data-testid='companies-search-search-field-location']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = 'Paris, France';", locationInput);

            var searchButton = driver.FindElement(By.CssSelector("[data-testid='companies-home-search-button-submit']"));
            searchButton.Click();
            Thread.Sleep(2000);

            var checkboxLabel = WaitForElement(driver, By.CssSelector("label.sc-eoVZPG"), 10);
            checkboxLabel.Click();
            Thread.Sleep(2000);

            // Liste des entreprises déjà traitées
            HashSet<string> processedLinks = new HashSet<string>();

            // Boucle pour traiter toutes les pages de résultats
            bool hasNextPage = true;
            while (hasNextPage)
            {
                // Récupérer la liste des entreprises sur la page actuelle
                var companyElements = driver.FindElements(By.CssSelector("li[data-testid='companies-search-search-results-list-item-wrapper'] a"));
                List<string> companyLinks = new List<string>();

                foreach (var element in companyElements)
                {
                    try
                    {
                        string link = element.GetAttribute("href");

                        // Ne garder que les liens se terminant par "/jobs"
                        if (!string.IsNullOrEmpty(link) && link.EndsWith("/jobs") && !processedLinks.Contains(link))
                        {
                            companyLinks.Add(link);
                            Console.WriteLine($"Nouvelle entreprise trouvée (Jobs) : {link}");
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Erreur : Impossible de récupérer un lien d'entreprise.");
                    }
                }

                Console.WriteLine($"Nombre total d'entreprises non traitées sur cette page : {companyLinks.Count}");

                // Ouvrir chaque lien pour postuler
                foreach (string link in companyLinks)
                {
                    try
                    {
                        // Ajouter le lien aux entreprises traitées
                        processedLinks.Add(link);

                        // Ouvrir le lien dans un nouvel onglet
                        ((IJavaScriptExecutor)driver).ExecuteScript("window.open(arguments[0]);", link);
                        Thread.Sleep(2000);

                        // Passer à l'onglet ouvert
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        Console.WriteLine($"Traitement de l'entreprise : {driver.Title}");

                        // Étape 4 : Cliquer sur "Postuler"
                        var applyButton = WaitForElement(driver, By.CssSelector("button[data-testid='company_jobs-button-apply']"), 10);
                        applyButton.Click();
                        Thread.Sleep(3000);

                        // Étape 5 : Remplir la lettre de motivation
                        var coverLetterTextarea = WaitForElement(driver, By.CssSelector("textarea[data-testid='apply-form-field-cover_letter']"), 10);
                        string coverLetter = @"Actuellement admis à CESI Nanterre pour préparer un Bachelor Concepteur
et Développeur d'Applications, je suis à la recherche d'une alternance pour
poursuivre ma formation en troisième année. Passionné par l'informatique,
avec une solide expérience en développement, gestion de projets
informatiques et support technique acquise lors de mon BTS Services
Informatiques aux Organisations (option SLAM), je suis motivé, dynamique et
prêt à m'intégrer rapidement au sein d'une équipe.";
                        coverLetterTextarea.SendKeys(coverLetter);

                        // Étape 6 : Cocher les deux cases pour accepter les termes
                        var termsCheckbox = WaitForElement(driver, By.CssSelector("input[data-testid='apply-form-terms']"), 10);
                        termsCheckbox.Click();

                        var consentCheckbox = WaitForElement(driver, By.CssSelector("input[data-testid='apply-form-consent']"), 10);
                        consentCheckbox.Click();

                        // Étape 7 : Cliquer sur "J’envoie ma candidature !"
                        var submitButton = WaitForElement(driver, By.CssSelector("button[data-testid='apply-form-submit']"), 10);
                        submitButton.Click();
                        Console.WriteLine($"Candidature envoyée avec succès pour l'entreprise : {link}");
                        Thread.Sleep(3000);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Erreur lors du traitement de l'entreprise : {link}, Erreur : {e.Message}");
                    }
                    finally
                    {
                        // Fermer l'onglet actuel et revenir au principal
                        driver.Close();
                        driver.SwitchTo().Window(driver.WindowHandles.First());
                    }
                }

                // Vérifier si un bouton "Page suivante" est présent
                try
                {
                    var nextPageButton = driver.FindElement(By.CssSelector("svg[alt='Right']"));
                    nextPageButton.Click();
                    Console.WriteLine("Passage à la page suivante.");
                    Thread.Sleep(3000);
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Aucune page suivante trouvée. Fin du traitement.");
                    hasNextPage = false;
                }
            }

            Console.WriteLine("Traitement terminé !");
        }
        catch (Exception e)
        {
            Console.WriteLine("Une erreur s'est produite : " + e.Message);
        }
        finally
        {
            driver.Quit();
        }
    }

    static IWebElement WaitForElement(IWebDriver driver, By by, int timeoutInSeconds)
    {
        for (int i = 0; i < timeoutInSeconds; i++)
        {
            try
            {
                var element = driver.FindElement(by);
                if (element.Displayed && element.Enabled)
                    return element;
            }
            catch (NoSuchElementException)
            {
                Thread.Sleep(1000);
            }
        }
        throw new NoSuchElementException($"L'élément avec le sélecteur {by.ToString()} n'a pas été trouvé après {timeoutInSeconds} secondes.");
    }
}
