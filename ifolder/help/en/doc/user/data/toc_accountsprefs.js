function updateMenu_accountsprefs() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/accountsprefs.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Starting the iFolder Client", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Starting the iFolder Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring Local Firewall Settings for iFolder Traffic", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0002.", "Configuring Local Firewall Settings for iFolder Traffic");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring an iFolder Account", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0003.", "Configuring an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Logging In to an iFolder Account", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0004.", "Logging In to an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing Information About an iFolder Account", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0005.", "Viewing Information About an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an iFolder Account", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0006.", "Deleting an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring iFolder Preferences for the Client", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0007.", "Configuring iFolder Preferences for the Client");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();