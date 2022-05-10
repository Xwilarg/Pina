# Pina
A Discord bot that allows anyone to pin messages in your server.

## Why would I need a bot for that?
If you want users to pin message, they must have the "Manage Messages" permission.<br/>
However, that includes the ability to delete messages, so it can be a bit risky to give it to everyone.<br/>
Thanks to this bot, anyone is now able to pin message by writing p.pin or by adding ?? emote to the corresponding message.

## Command list

|Command|Description|Example|
|:--|:--|:--|
|`pin [(optional) message/user ID]`| Pin a message.|pin 000000000000000000|
|`unpin [message ID]`| Unpin a message.|unpin 000000000000000000|
|`language [language name]`| Set my speaking language.|language french|
|`verbosity [none/error/info]`| Set if I say something or not when something occurs.|verbosity none|
|`whitelist [(optionnal)roles]`| Set the roles that can pin messages, don't write anything for all.|whitelist @MyRole|
|`blacklist [(optionnal)roles]`| Set the users that can't pin messages, don't write anything for none, admins are not affected by this.|blacklist @MyRole|
|`prefix [(optional)prefix]`| Set the prefix for bot command, don't write anything to allow the use of command without one.|prefix p!|
|`botinteract [true/false]`| Set if other bots are allowed to do commands.|botinteract false|
|`canunpin [true/false]`| Set if users can unpin messages.|canunpin false|
|`voterequired [votes required]`| Set the number of people that need to vote to pin/unpin a message, set to 1 to disable.<br/>You can also pin message by adding the 📌 reaction to a message, and unpin them by adding a ⛔ reaction.|voterequired 2|
|`gdpr`| Display the information I have about this guild.||
|`info`| Display various information about me.||
|`invite`| Display the invite link of the bot.||

## Invite Pina to your server
Just click [here](https://discord.com/api/oauth2/authorize?client_id=583314556848308261&scope=bot%20applications.commands).

## Contact me
Have any question?<br/>
You can either contact me on Discord (Zirk#0001) or by Email ([xwilarg@yahoo.fr](mailto:contact@zirk.eu))

## Anything else?
Yes, Pina is also released on top.gg so go check [her page](https://top.gg/bot/583314556848308261).
