# Schachbot
Das ist ein Kollab-Projekt von mir und Wutpups, welches wir auf [stackstream](https://stack-stream.com/) streamen.
Falls Interesse am Projekt besteht und wie das ganze angefangen hat, könnt ihr jeweils auf mein ([TheAmazingLooser](https://stack-stream.com/profile/TheAmazingLooser)) und [Wutpups](https://stack-stream.com/profile/Wutpups)' Profil die bereits vergangenen Streams zu diesem Projekt anschauen.

# Wie bringe ich das Teil zum laufen?
Das Projekt wird in C# entwickelt. Genauer verwendet es das .NET6 Backend. Da .NET6 die letzte LTS-Version von .NET ist.
Das könnte sich jedoch noch ändern, weil FNA selbst auf .NET7 entwickelt wird.
Zum Kompilieren braucht ihr also ein installiertes .NET6 (Development Kit) und betenfalls Roslyn oder eine IDE nach wahl (die, wenn sie C# unterstützen, auch oft den Roslyn-Compiler mitliefern).
Folgende befehle sind notwendig um an die vollständigen Sources zu kommen:
```
git clone --recurse-submodules https://github.com/TheAmazingLooser/Schachbot.git
git submodule update --init --recursive
```

Dabei kann es bei alten git-versionen unterschiede geben:
```
git clone --recursive https://github.com/TheAmazingLooser/Schachbot.git
git submodule update --init --recursive
git submodule update --recursive
```
Bei sehr alten Git-Versionen könnte das `--init` schon bereits intitialisierte Submodule nicht updaten. Deswegen wird der Befehl auch nochmal ohne das `--init`-Flag ausgeführt.

Bei einem Bereits geclonten Projekt reichen folgende Befehle aus:
```
git pull
git submodule update --init --recursive
git submodule update --recursive
```
