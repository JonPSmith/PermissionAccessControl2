# PermissionAccessControl2

Welcome to version 2 of the example application that contains my feature and data authorization code. This pretends to be a SaaS application which provides stock and sales management to companies with multiple retail outlets.  
This is open-source application (MIT license).

## See the articles

* Part 3: A better way to handle authorization – six months on (coming soon!).
* Part 4: Handling data authorization - six months on (coming soon).
* Part 5: A better way to handle authorization – refreshing users claims (coming soon).

## How to play with the application

The default setting (see Configuration section below) will use in-memory databases which it will preload with demo users and data at startup. The demo users have:
1. Different **Permissions**, which controls what they can do, e.g. only a StoreManager can provide a refund.
2. Different **DataKey**, which controls what part of the shop data they can see, e.g. a SalesAssistant and StoreManager can only see the data in their shop, but a Director can see all shop data in the company.

The home page will show you a list of users that you can log in via (the email address is also the password). There are two different companies, 4U Inc. and Pets2 Ltd., which have a number of shops in different divisions, represented by hierarchical data. Logging in as a user will give you access to some features and data (if linked to data).

Once you have logged in you will be sent to an appropriate page: if the user has the "StockSell" permission your will be taken to the "Shop Till" page, otherwise you will be taken to the "Sales" page.

## Configuration

The [appsetting.json file](https://github.com/JonPSmith/PermissionAccessControl2/blob/master/PermissionAccessControl2/appsettings.json) contains settings that configure how the system runs.

### Setting up SuperAdmin user


### Controlling how the demo works



