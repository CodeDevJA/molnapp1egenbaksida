{
(
  http://localhost:5000              // Lokal ASP.NET 8 Web App (via Kestrel eller IIS Express)
  http://127.0.0.1:5000              // Samma som ovan, via IP-adress

  http://localhost:7071              // Azure Function App med HTTP Trigger (lokal emulator)
  http://127.0.0.1:7071              // Samma som ovan, via IP-adress

  http://localhost:3000              // React, Vite eller annan JS frontend (dev-server)
  http://127.0.0.1:3000              // Samma som ovan, via IP-adress

  http://localhost:5500              // Live Server (VS Code extension för HTML/JS)
  http://127.0.0.1:5500              // Samma som ovan, via IP-adress

  http://localhost:8000              // Lokal testserver (for local testing), t.ex. Python (Django, Flask) eller Parcel
  http://127.0.0.1:8000              // Samma som ovan (for local testing), via IP-adress

  http://localhost                   // Bruno API Client (desktop-app som använder WebView)

  https://yourapp.azurewebsites.net         // Azure App Service (Web App i produktion/staging)
  https://yourfunction.azurewebsites.net    // Azure Function App (serverless API i molnet)
  https://yourapp.z01.azurestaticapps.net   // Azure Static Web Apps (frontend + API-hosting)

  https://your-username.github.io           // GitHub Pages (statisk frontend från repo)
  https://yourproject.vercel.app            // Vercel-hostad app (React/Vite/JS, import från GitHub)
)
}

Tips: 
Byt ut yourapp, yourfunction, your-username, och yourproject 
till dina faktiska namn/domäner 
innan du lägger in dem i Azure Portal → Function App → API → CORS.
