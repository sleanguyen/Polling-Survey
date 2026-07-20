# Polling-Survey
Steps to deploy this project to Live Service (Covering Render for backend and Vercel for frontend). Remember to clone and connect the repository to Render and Railway.
# Render
1. Create a Postgres database service. Save the internal connection url (can be found and regenerate later on). REGION MUST MATCH THE WEBSERVICE.
2. Create a KeyValue service (necessary for Redis). Save the internal keyvalue url (can be found and regenerate later on). REGION MUST MATCH THE WEBSERVICE.
3. Create a Web Service.
4. Set root directory to "backend".
5. Dockerfile path to "PollingSurvey.API/Dockerfile".
6. Docker build directory to "." .
7. Create the following environment variables, get the value from the respective service.
8. ConnectionStrings__DefaultConnection = Host=<Hostname>;Port=<port>;Database=<Database>;Username=<Username>;Password=<Password>;SSL Mode=Require;Trust Server Certificate=true
9. ConnectionStrings__Redis = red-<value>
10. DOTNET_USE_POLLING_FILE_WATCHER = 1
11. FrontendBaseUrl = https://<frontend-name>.vercel.app/poll (can be set after deploying frontend)
12. Deploy
# Vercel
1. Set Framework (under  Production Override to Vite)
2. Set environment variable
3. VITE_API_BASE_URL = <backend-url,like https://backendnamedonotuse.onrender.com/>
4. Deploy
