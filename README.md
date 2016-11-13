# nds-fake-cia-generator

This program grabs a .nds rom dump (commercial or not) and creates a fake CIA using it's icon and name. That fake CIA will boots your slot-1 flashcart.

It's made by using [Apache Thunder's TWL Slot-1 Launcher](https://gbatemp.net/threads/twl-slot-1-launcher-first-custom-dsiware-app.414501/), but injecting into it the new icon and name (and also the Title ID), and then making a CIA using make_cia due to it's actually a DSiWare App.

Using this you can have your favourites NDS games icons in the homescreen of your 3DS, and if you have a flashcart, you can launch that games and they'll just boot you flashcart. This is a *fake* alternative to [DS Forwarders](https://gbatemp.net/threads/nds-forwarder-cias-for-your-home-menu.426174/) if you have an unsupported flashcart or if you can't use the Real Time Menu by using those forwarders. Also, you'll see the games in the Activity Menu, so you can track them.


Thanks to:
* [Apache Thunder](https://github.com/ApacheThunder) for TWL Slot-1 Launcher.
* [Tiger](https://github.com/Tiger21820) for make_cia tool.
