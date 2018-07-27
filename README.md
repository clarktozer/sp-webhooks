# sp-webhooks
Complete .Net core web api project for consuming SharePoint webhooks. Includes database setup using entity framework for change token storage, and a REST endpoint for the subscriptions to POST to.

## Deploy Instructions for Azure
What you need: App service, SQL server and database, service bus namespace and queue.

Grab all connection strings and the queue name and enter them into the appsettings.json files or override them in the app services settings through the portal.

Generate the DB from entity framework migrations.

Publish the Web project to the app service.

Publish the web job to the app services web jobs (zip up the debug folder in bin and manually upload through Azure)

In order to see the output of the processing of the messages, look at the KUDU logs for the webjob. This will print the changes to the list that the webhook is subscribed to.
