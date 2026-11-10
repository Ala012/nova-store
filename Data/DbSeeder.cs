using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TP_Product.Models;

namespace TP_Product.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await db.Database.EnsureCreatedAsync();

        foreach (var role in new[] { "Admin", "Manager", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await EnsureUserAsync(userManager, "admin@nova.tn", "Admin@123", "Admin");
        var manager = await EnsureUserAsync(userManager, "manager@nova.tn", "Manager@123", "Manager");
        var client = await EnsureUserAsync(userManager, "client@nova.tn", "Client@123", "User");
        await EnsureUserAsync(userManager, "sami@nova.tn", "Sami@123", "User");

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { CategoryName = "Audio", Icon = "bi-headphones", Accent = "#7c5cff", Description = "Casques, écouteurs et enceintes pour une immersion totale." },
                new Category { CategoryName = "Montres", Icon = "bi-smartwatch", Accent = "#f59e0b", Description = "Montres connectées et classiques au caractère affirmé." },
                new Category { CategoryName = "Informatique", Icon = "bi-laptop", Accent = "#3b82f6", Description = "Ordinateurs, écrans et accessoires de productivité." },
                new Category { CategoryName = "Maison", Icon = "bi-house-heart", Accent = "#10b981", Description = "Objets déco et essentiels pour un intérieur chaleureux." },
                new Category { CategoryName = "Mode", Icon = "bi-handbag", Accent = "#ec4899", Description = "Sacs, sneakers et accessoires qui font la différence." },
                new Category { CategoryName = "Sport", Icon = "bi-bicycle", Accent = "#f97316", Description = "Équipement pour bouger mieux, chaque jour." });
            await db.SaveChangesAsync();
        }

        if (!await db.Products.AnyAsync())
        {
            var categories = await db.Categories.ToDictionaryAsync(c => c.CategoryName, c => c.CategoryId);
            var now = DateTime.UtcNow;

            var products = new List<Product>
            {
                NewProduct("Casque NovaSound Pro", 349m, 429m, 24, "Audio", "audio-casque.svg",
                    "Casque sans fil à réduction de bruit active, 40 h d'autonomie et son spatial signé NovaSound.", true, false, now.AddDays(-6), categories),
                NewProduct("Écouteurs AirPulse", 189m, 239m, 38, "Audio", "audio-ecouteurs.svg",
                    "Écouteurs true wireless ultra-légers, charge rapide et qualité audio cristalline.", true, true, now.AddDays(-2), categories),
                NewProduct("Enceinte BoomMini", 259m, null, 16, "Audio", "audio-enceinte.svg",
                    "Enceinte portable étanche IPX7 avec basses profondes et 18 h de lecture.", false, true, now.AddDays(-1), categories),
                NewProduct("Montre Nova Chrono", 599m, 699m, 12, "Montres", "montre-chrono.svg",
                    "Montre connectée AMOLED, GPS intégré, suivi du sommeil et 7 jours d'autonomie.", true, false, now.AddDays(-12), categories),
                NewProduct("Montre Sport Fit", 429m, null, 21, "Montres", "montre-sport.svg",
                    "Compagnon sportif avec 40 modes d'entraînement et étanchéité 5 ATM.", false, true, now.AddDays(-3), categories),
                NewProduct("Montre Classique Or", 749m, 899m, 8, "Montres", "montre-or.svg",
                    "Boîtier acier doré, verre saphir et bracelet cuir italien. L'élégance intemporelle.", true, false, now.AddDays(-20), categories),
                NewProduct("Laptop NovaBook Air", 3299m, 3699m, 7, "Informatique", "info-laptop.svg",
                    "Ultrabook 14\" OLED, processeur dernière génération, 16 Go RAM et 1 To SSD.", true, true, now.AddDays(-4), categories),
                NewProduct("Clavier Mech RGB", 289m, null, 34, "Informatique", "info-clavier.svg",
                    "Clavier mécanique hot-swap, switches silencieux et rétroéclairage RGB personnalisable.", false, false, now.AddDays(-18), categories),
                NewProduct("Souris Glide Pro", 149m, 179m, 45, "Informatique", "info-souris.svg",
                    "Souris sans fil 26K DPI, 70 g, autonomie 90 h et glisse parfaite.", false, false, now.AddDays(-22), categories),
                NewProduct("Écran UltraWide 34\"", 1899m, null, 9, "Informatique", "info-ecran.svg",
                    "Moniteur incurvé 34\" QHD 165 Hz, HDR400 et connectique USB-C 90 W.", true, false, now.AddDays(-9), categories),
                NewProduct("Lampe Aura", 119m, null, 27, "Maison", "maison-lampe.svg",
                    "Lampe de table tactile avec 16 millions de couleurs et mode relaxation.", false, true, now.AddDays(-2), categories),
                NewProduct("Chaise Nordique", 459m, 529m, 11, "Maison", "maison-chaise.svg",
                    "Fauteuil scandinave en chêne massif et textile bouclé, confort durable.", true, false, now.AddDays(-15), categories),
                NewProduct("Plante Monstera", 79m, null, 32, "Maison", "maison-plante.svg",
                    "Monstera Deliciosa livrée en pot céramique mat. La touche verte qui change tout.", false, false, now.AddDays(-5), categories),
                NewProduct("Diffuseur Zen", 99m, 129m, 19, "Maison", "maison-diffuseur.svg",
                    "Diffuseur d'huiles essentielles ultrasonique, minuterie et lumière d'ambiance.", false, false, now.AddDays(-11), categories),
                NewProduct("Sac Urban Leather", 389m, 459m, 14, "Mode", "mode-sac.svg",
                    "Sac en cuir pleine fleur, compartiment laptop 14\" et finitions main.", true, false, now.AddDays(-8), categories),
                NewProduct("Sneakers Cloud", 329m, null, 26, "Mode", "mode-sneakers.svg",
                    "Sneakers à semelle nuage, mesh respirant et design épuré pour tous les jours.", false, true, now.AddDays(-3), categories),
                NewProduct("Lunettes Solar", 199m, 249m, 23, "Mode", "mode-lunettes.svg",
                    "Lunettes de soleil polarisées, monture acétate et protection UV400.", false, false, now.AddDays(-17), categories),
                NewProduct("Tapis Yoga Flow", 129m, null, 30, "Sport", "sport-yoga.svg",
                    "Tapis 6 mm antidérapant en TPE écologique, avec sangle de transport.", false, false, now.AddDays(-13), categories),
                NewProduct("Haltères 10 kg", 179m, 219m, 13, "Sport", "sport-halteres.svg",
                    "Paire d'haltères réglables 2 x 10 kg, revêtement anti-choc et poignée ergonomique.", false, false, now.AddDays(-7), categories),
                NewProduct("Gourde Hydro", 59m, null, 52, "Sport", "sport-gourde.svg",
                    "Gourde isotherme 750 ml, inox double paroi, 24 h de fraîcheur.", false, true, now.AddDays(-1), categories)
            };

            db.Products.AddRange(products);
            await db.SaveChangesAsync();
        }

        if (!await db.Reviews.AnyAsync())
        {
            var products = await db.Products.OrderBy(p => p.ProductId).ToListAsync();
            var reviewers = new[]
            {
                ("seed-1", "Sami Ben Ali"), ("seed-2", "Ines Trabelsi"), ("seed-3", "Youssef K."),
                ("seed-4", "Mariem Gharbi"), ("seed-5", "Amine R."), ("seed-6", "Lina Belhadj")
            };
            var comments = new[]
            {
                "Qualité au top, je recommande sans hésiter !",
                "Très beau produit, conforme à la description. Livraison rapide.",
                "Excellent rapport qualité/prix. Je suis conquis.",
                "Design soigné et finitions impeccables. Petit coup de cœur.",
                "Bon produit dans l'ensemble, emballage très soigné.",
                "Parfait pour mon usage quotidien, rien à redire."
            };

            var random = new Random(42);
            var reviews = new List<Review>();

            for (var index = 0; index < products.Count; index++)
            {
                var product = products[index];
                var count = index % 3 + 1;

                for (var r = 0; r < count; r++)
                {
                    var reviewer = reviewers[(index + r) % reviewers.Length];
                    var rating = index % 5 == 0 && r == 0 ? 4 : 5;
                    reviews.Add(new Review
                    {
                        ProductId = product.ProductId,
                        UserId = reviewer.Item1,
                        UserName = reviewer.Item2,
                        Rating = rating,
                        Comment = comments[(index + r) % comments.Length],
                        CreatedAt = DateTime.UtcNow.AddDays(-(index + r + 1))
                    });
                }

                product.RatingCount = count;
                product.RatingAverage = reviews.Where(x => x.ProductId == product.ProductId).Average(x => x.Rating);
            }

            db.Reviews.AddRange(reviews);
            await db.SaveChangesAsync();
        }

        if (!await db.Orders.AnyAsync() && client is not null)
        {
            var products = await db.Products.OrderBy(p => p.ProductId).Take(12).ToListAsync();
            var statuses = new[]
            {
                OrderStatuses.Delivered, OrderStatuses.Shipped, OrderStatuses.Pending,
                OrderStatuses.Confirmed, OrderStatuses.Cancelled, OrderStatuses.Delivered,
                OrderStatuses.Delivered, OrderStatuses.Shipped
            };
            var names = new[] { "Client Nova", "Sami Ben Ali", "Ines Trabelsi", "Youssef K.", "Mariem Gharbi", "Amine R.", "Lina Belhadj", "Client Nova" };
            var addresses = new[] { "12 rue du Lac, Tunis", "Avenue Habib Bourguiba, Sousse", "Rue de la Liberté, Sfax", "Les Berges du Lac 2, Tunis", "Avenue de la République, Bizerte", "Rue Ibn Khaldoun, Nabeul", "Zone touristique, Hammamet", "Rue de Marseille, Tunis" };
            var payments = new[] { "Carte bancaire", "PayPal", "Paiement à la livraison" };

            for (var i = 0; i < statuses.Length; i++)
            {
                var chosen = products.Skip(i * 2 % 8).Take(2 + (i % 3)).ToList();
                var items = chosen.Select((p, index) => new OrderItem
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name,
                    Image = p.Image,
                    Quantity = 1 + ((i + index) % 2),
                    Price = p.Price
                }).ToList();

                var order = new Order
                {
                    CustomerName = names[i],
                    Email = i == 0 ? client.Email! : $"client{i}@nova.tn",
                    Address = addresses[i],
                    Phone = $"+216 5{i} 234 56{i}",
                    PaymentMethod = payments[i % payments.Length],
                    Status = statuses[i],
                    OrderDate = DateTime.Now.AddDays(-(7 - i)).AddHours(-(i * 2)),
                    UserId = client.Id,
                    Items = items,
                    TotalAmount = items.Sum(x => x.LineTotal)
                };

                db.Orders.Add(order);
            }

            await db.SaveChangesAsync();
        }
    }

    private static Product NewProduct(
        string name, decimal price, decimal? oldPrice, int stock, string categoryName, string image,
        string description, bool featured, bool isNew, DateTime createdAt, Dictionary<string, int> categories)
    {
        return new Product
        {
            Name = name,
            Price = price,
            OldPrice = oldPrice,
            QteStock = stock,
            CategoryId = categories[categoryName],
            Image = "products/" + image,
            Description = description,
            IsFeatured = featured,
            IsNew = isNew,
            CreatedAt = createdAt
        };
    }

    private static async Task<IdentityUser?> EnsureUserAsync(
        UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return null;
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);

        return user;
    }
}
