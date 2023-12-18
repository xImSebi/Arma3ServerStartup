# Arma3ServerStartup

## Zur Ausführung wird .NET 6.0 benötigt ([x64 .NET 6 Download](https://link-url-here.org](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.25-windows-x64-installer)https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.25-windows-x64-installer))

## Instructions:
* Download the Arma3ServerStartup.exe file from the [latest Release](https://github.com/xImSebi/Arma3ServerStartup/releases)  
* Run the Arma3ServerStartup.exe file
* Wait 15s until the config.json is created
* Close the Console Window
* Edit the config.json to your likings
* Optional: Create files for the modlist and optional modlist (Note: the mod-keys needs to be in a directory named "keys" (Case Insensitive) inside the mod directory)
* Startup the Arma3ServerStartup.exe again
* You're done!


## Config.json
  `name`: This value is just a descriptor. It can be anything you want.  
  `serverPath`: The path where your arma3server_x64.exe is located at. For example: "C:\\arma"  
  `serverConfigPath`: The path to your server.cfg file. For example: "C:\\arma\\server.cfg"  
  `battleyePath`: The path to your BattlEye directory, including your BattlEye config. For example: "C:\\arma\\battleye"  
  `profilesPath`: The path where your server profile and rpt logs are located at. (Server Profile is used to set custom difficulty) For example: "C:\\arma\\profiles"  
  `modsPath`: The path where your mods are located at. For example if you downloaded the mods with steamcmd: "E:\\steamcmd\\steamapps\\workshop\\content\\107410"  
  `modListPath`: The path to a file containing the directory names for the mods. For example: "E:\\serverstartup\\modlist.txt"  
  `allowedModListPath`: The path to a file containing the directory names for mods that are allowed on your server. For example: "E:\\serverstartup\\modlist_optional.txt"  
  `additionalStartupParameters`: Additional parameters you want your server to use on startup. For example: "-netlog -autoInit -port=2302 -name=server -cfg=Basic.cfg"  

## Modlist example:
If you downloaded the mods via steamcmd, the mod directories are saved with the mod-workshop-id. A modlist.txt file would then look like this:
> `450814997`  
> `463939057`  
> `894678801`  
> `1779063631`  

If you downloaded the mods via steamcmd and changed their name to describe the mod better, or downloaded it with the steam desktop app, a modlist.txt file would look like this:
> `@CBA_A3`  
> `@ace`  
> `@Task Force Arrowhead Radio (BETA!!!)`  
> `@Zeus Enhanced`

Same for the allowed-mods file.
