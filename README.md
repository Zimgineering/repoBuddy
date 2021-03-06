
# repoBuddy
A plugin for the RebornBuddy FFXIV MMO bot to automatically update accessories from repositories.

[Included in repoBuddyRepos.xml](#included-repos) are a number of commonly used repos, if you do not want any of those then remove the relevant data in the xml (using a text editor) or using the settings button before enabling.
# Installation
SharpSvn requires Microsoft Visual C++ 2010 SP1 Redistributable Package to function, download [HERE (64-bit)](https://www.microsoft.com/en-us/download/details.aspx?id=13523)
1. Download using your preferred method or [HERE](https://github.com/Zimgineering/repoBuddy/archive/master.zip)
    >make sure to unblock the file (properties->unblock checkbox)
2. Extract the archive into your plugins directory (remove -master from the folder name if necessary)
    >should look like this: Rebornbuddy/Plugins/repoBuddy/sharpSVN.dll
3. Delete any old repositories that might be in the included repoBuddyRepos.xml to prevent errors and namespace conflicts
4. Enable repoBuddy in your plugins section ~~and restart rebornbuddy when the process is complete for changes to take effect~~ v0.0.0.2+ forces a restart after a botbase/plugin/routine gets updated.
5. Click any of the direct download buttons on the One-Time downloads tab, and once finished click Restart RebornBuddy.

# Included Repos

**Botbases**:  
[DeepDive-Combined](https://github.com/nt153133/DeepDive) - Palace of the Dead and Heaven on High botbase. Don't run HoH solo.  
[Kombatant](https://github.com/Fryheit/Kombatant) - Much better combat botbase, features autofollowing etc.  
[LizExFateBot](https://github.com/jamharri/ExFateBot) - Enhanced fatebot, requires [Lisbeth](https://www.siune.io/products/lisbeth).  
[LlamaLibrary](https://github.com/nt153133/LlamaLibrary) - Mostly for developers, Angles24 profiles require this. Includes botbases for desynth/retainers/materia/autotrade/autofollow.  

**Plugins**:  
[AlertMe](https://github.com/parrot-dev/AlertMe) - Chat/emote monitoring.  
[CarbuncleTech](https://github.com/Fryheit/CarbuncleTech) - Mostly for SeleCR but includes a zoom hack and titlebar changer.  
[ExBuddy](https://github.com/Entrax643/ExBuddy) - Adds Enable Flight plugin and behaviors required for Mew and certain profiles.  
[LeveGen](https://github.com/zzi-zzi-zzi/LeveGen) - Generates leve profiles.  
[LisbethVentures](https://github.com/nt153133/LisbethVentures) - Reactivates retainer ventures using Lisbeth hooks.
[RBtrust](https://github.com/athlon18/RBtrust) - Trust plugin; enable the plugin and load a profile from the RBtrust folder.  
[repoBuddy](https://github.com/Zimgineering/repoBuddy) - Auto-updating your auto-updater!  

**Profiles**:  
[Angles24/DoH80-DoL80](https://github.com/Angles24/DoH80-DoL80) - Profiles to level your DoH/DoL, requires [LlamaLibrary](https://github.com/nt153133/LlamaLibrary) and [Lisbeth](https://www.siune.io/products/lisbeth).  
[Sodimm](https://github.com/sodimm/RebornBuddy/tree/master/Profiles/Sodimm) - Profiles for dailies, MSQ, etc.  
[Y2Krazy](https://github.com/y2krazy/RebornBuddy-Profiles) - Anything not in Sodimm's are likely here.  
[ZodiacWeapons](https://github.com/domesticwarlord86/ZodiacWeapons) - Automate the zodiac weapon aquisition and upgrades.

# Warnings
* You will lose local changes to files when an update is processed, but unversioned files should not be lost.
* If you run into any issues please try a clean checkout by either deleting the repo folder or the hidden .svn folder and restarting RebornBuddy


![](Images/repoBuddyGUI.png)
![](Images/repoBuddyGUI2.png)
![](Images/repoBuddyLog.png)


# About SharpSVN
[SharpSvn](https://sharpsvn.open.collab.net/) is a binding of the Subversion Client API for .Net 2.0-4.0+ applications contained within a set of xcopy-deployable dll's and is licensed under the Apache 2.0 license, to allow using it in both open source and commercial projects 


# Special thanks to
[Kayla D'orden](https://github.com/nt153133)
