# Going live

Everything the code needs is already in place. Going live is accounts, settings and DNS.
Do the steps in order; each one says where its values come from.

## 1. Accounts

| What | Used for | Notes |
|---|---|---|
| Domain (e.g. `prodify.ng`) | Website and API addresses, email sender | `.ng` / `.com.ng` from a Nigerian registrar, `.com` from Cloudflare or Namecheap |
| Cloudinary | Product photos | Free plan is enough to start |
| Brevo | Emails (order updates, password reset) | Free daily allowance |
| Hosting for the API and database | Runs the API (Docker image) and SQL Server | Azure App Service + Azure SQL, or one Linux server (VPS) |
| Cloudflare Pages | Runs the website | Free |

## 2. Addresses

| Address | Points to |
|---|---|
| `https://prodify.ng` (and `www`) | The website (Cloudflare Pages) |
| `https://api.prodify.ng` | The API |

Both must be HTTPS. Azure and Cloudflare Pages issue the certificates; on a VPS use Caddy or Nginx with Let's Encrypt.

## 3. Database

1. Create an empty SQL Server database, e.g. `Prodify`.
2. Create a login just for the API (not `sa`) that owns that database.
3. Note the connection string, e.g.
   `Server=YOUR-SERVER;Database=Prodify;User Id=prodify_api;Password=...;Encrypt=True;`

The API creates all tables itself on first start (see `Database__MigrateOnStartup` below).
Turn on the host's automatic daily backups.

## 4. API settings

Set these as environment variables on the host (two underscores = a section, e.g. `Email:Mode`).
The API refuses to start, and lists what is missing, until the required ones are set.

| Setting | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | The connection string from step 3 |
| `Jwt__SecretKey` | A new random key, see below. Never reuse the one in appsettings.json |
| `Cors__AllowedOrigins__0` | `https://prodify.ng` |
| `Cors__AllowedOrigins__1` | `https://www.prodify.ng` (if you use www) |
| `App__FrontendUrl` | `https://prodify.ng` (links in emails) |
| `FileStorage__CloudinaryUrl` | Cloudinary dashboard: "API environment variable", `cloudinary://...` |
| `Email__Mode` | `Smtp` |
| `Email__SmtpHost` | `smtp-relay.brevo.com` |
| `Email__SmtpPort` | `587` |
| `Email__SmtpUser` | Brevo: SMTP & API page, "Login" |
| `Email__SmtpPassword` | Brevo: SMTP & API page, a new SMTP key |
| `Email__FromAddress` | `noreply@prodify.ng` |
| `Email__FromName` | `Prodify` |
| `Database__MigrateOnStartup` | `true` (each deploy updates the database before the API starts) |
| `ReverseProxy__Enabled` | `true` on Azure App Service or behind Nginx/Caddy |
| `SeedAdmin__Email` | Your admin login, first start only |
| `SeedAdmin__Password` | A strong password, first start only |

Make the JWT key in PowerShell:

```powershell
$b = New-Object byte[] 48; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

Optional: `RateLimits__AuthPerMinute` (default 20) and `RateLimits__GeneralPerMinute` (default 600) per visitor IP.

## 5. Deploy the API

The Docker image is built from the repository root:

```powershell
docker build -f Infrastructure/docker/api/Dockerfile --target runtime -t prodify-api .
```

It listens on port 8080 and runs as a normal (non-root) user. Health check: `GET /health`.

On the first start the API creates the tables, the roles and your admin account.
Then remove `SeedAdmin__Password` from the settings (the account stays).
No demo products or demo accounts are created on a live server.

## 6. Deploy the website (Cloudflare Pages)

Connect the `prodify-web` repository and use:

| Setting | Value |
|---|---|
| Build command | `npm run build` |
| Output folder | `dist` |
| Environment variable | `VITE_API_BASE_URL` = `https://api.prodify.ng/api` |

`public/_redirects` makes every page address load the app, and `public/_headers` adds
security headers and long caching for `/assets`. If the build log says
"VITE_API_BASE_URL is not set", the variable is missing.

## 7. Email (Brevo)

1. In Brevo, add `prodify.ng` as a sender domain.
2. Add the DNS records Brevo shows (DKIM, and its SPF/DMARC advice) at your domain registrar.
3. Wait until Brevo shows the domain as verified, then create the SMTP key for step 4.

Without a verified domain, emails go to spam or are refused.

## 8. Check it works

- [ ] `https://api.prodify.ng/health` shows Healthy.
- [ ] The website opens, products and categories load, no errors in the browser console.
- [ ] Register a customer account; the welcome email arrives (check spam too).
- [ ] Forgot password: the email arrives and the link works.
- [ ] As a seller: apply, get approved by the admin, add a product with a photo
      (the photo address should start with `https://res.cloudinary.com/`).
- [ ] Place an order, move it through confirm, pack, ship and deliver.
- [ ] Admin dashboard and sales chart show the order.

## 9. After launch

- Check the host's logs for errors in the first days.
- Keep the database backups on, and try restoring one once.
- Payments are still simulated: the next step is Paystack (test keys first, then live keys).
