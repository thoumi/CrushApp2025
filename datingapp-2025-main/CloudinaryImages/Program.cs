using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Xml;

class Profil
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Gender { get; set; }
    public string DateOfBirth { get; set; }
    public string DisplayName { get; set; }
    public string Created { get; set; }
    public string LastActive { get; set; }
    public string Description { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string ImageUrl { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // 1. Configuration Cloudinary
        var account = new Account(
            "TON_CLOUD_NAME",
            "TON_API_KEY",
            "TON_API_SECRET"
        );
        var cloudinary = new Cloudinary(account);

        // -------------------
        // ÉTAPE A : TAGGING AUTO
        // -------------------

        // Taguer les images du dossier women/ en female
        var women = cloudinary.ListResources(new ListResourcesParams()
        {
            Prefix = "women/",
            MaxResults = 100
        });

        foreach (var res in women.Resources)
        {
            cloudinary.AddTag("female", new string[] { res.PublicId });
            Console.WriteLine($"✅ {res.PublicId} tagué female");
        }

        // Taguer les images du dossier men/ en male
        var men = cloudinary.ListResources(new ListResourcesParams()
        {
            Prefix = "men/",
            MaxResults = 100
        });

        foreach (var res in men.Resources)
        {
            cloudinary.AddTag("male", new string[] { res.PublicId });
            Console.WriteLine($"✅ {res.PublicId} tagué male");
        }

        Console.WriteLine("🎉 Tagging terminé !");

        // -------------------
        // ÉTAPE B : MISE À JOUR DU JSON
        // -------------------

        // Charger le JSON de profils
        var jsonPath = "profils.json";
        var profils = JsonConvert.DeserializeObject<List<Profil>>(File.ReadAllText(jsonPath));

        // Fonction pour récupérer N URLs avec transformation
        List<string> GetImageUrls(string tag, int n)
        {
            var result = cloudinary.ListResourcesByTag(tag, new ListResourcesByTagParams()
            {
                MaxResults = 50
            });

            var urls = result.Resources.Select(r =>
                cloudinary.Api.UrlImgUp
                    .Transform(new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"))
                    .BuildUrl(r.PublicId)
            ).ToList();

            return urls.Take(n).ToList();
        }

        // Récupérer 15 femmes et 15 hommes
        var femaleUrls = GetImageUrls("female", 15);
        var maleUrls = GetImageUrls("male", 15);

        int fIndex = 0, mIndex = 0;

        foreach (var p in profils)
        {
            if (p.Gender == "female" && fIndex < femaleUrls.Count)
            {
                p.ImageUrl = femaleUrls[fIndex++];
            }
            else if (p.Gender == "male" && mIndex < maleUrls.Count)
            {
                p.ImageUrl = maleUrls[mIndex++];
            }
        }

        // Sauvegarder le nouveau JSON
        var updatedJson = JsonConvert.SerializeObject(profils, Formatting.Indented);
        File.WriteAllText("profils_updated.json", updatedJson);

        Console.WriteLine("✅ Profils mis à jour dans profils_updated.json");
    }
}
