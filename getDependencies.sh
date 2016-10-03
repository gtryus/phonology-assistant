#!/bin/bash
# server=build.palaso.org
# project=PhonologyAssistant
# build=bt223
# root_dir=..
# Auto-generated by https://github.com/chrisvire/BuildUpdate.
# this file has been edited by hand after being autogenerated! Currently Windows and Linux are included in the same file.

cd "$(dirname "$0")"

# *** Functions ***
force=0
clean=0

while getopts fc opt; do
case $opt in
f) force=1 ;;
c) clean=1 ;;
esac
done

shift $((OPTIND - 1))

copy_auto() {
if [ "$clean" == "1" ]
then
echo cleaning $2
rm -f ""$2""
else
where_curl=$(type -P curl)
where_wget=$(type -P wget)
if [ "$where_curl" != "" ]
then
copy_curl $1 $2
elif [ "$where_wget" != "" ]
then
copy_wget $1 $2
else
echo "Missing curl or wget"
exit 1
fi
fi
}

copy_curl() {
echo "curl: $2 <= $1"
if [ -e "$2" ] && [ "$force" != "1" ]
then
curl -# -L -z $2 -o $2 $1
else
curl -# -L -o $2 $1
fi
}

copy_wget() {
echo "wget: $2 <= $1"
f=$(basename $2)
d=$(dirname $2)
cd $d
wget -q -L -N $1
cd -
}


# *** Results ***
# build:   pathway-win32-default continuous (bt50)
# project: pathway
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt50
# VCS: https://github.com/sillsdev/pathway [default]
# dependencies:
# [0] build:  L10NSharp continuous (bt196)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/dotnet", "L10NSharp.pdb"=>"lib/dotnet"}
#     VCS: https://bitbucket.org/sillsdev/l10nsharp [master]
# or
# build:    pathway-precise-64-continuous (bt362)
# project: pathway
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt362
# VCS: https://github.com/sillsdev/pathway [default]
# dependencies:
# [0] build:  L10NSharp Mono continuous (bt271)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/dotnet", "L10NSharp.pdb"=>"lib/dotnet"}
#     VCS: https://bitbucket.org/sillsdev/l10nsharp [master]

# make sure output directories exist
if [[ "$OSTYPE" == "linux-gnu" ]]; then
       echo "Linux"
	   mkdir -p ../Installer/
	   mkdir -p DistFiles/Linux

		# download artifact dependencies
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt324/phonologyAssistant.tcbuildtag/L10NSharp.dll DistFiles/Linux/L10NSharp.dll
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt324/phonologyAssistant.tcbuildtag/L10NSharp.pdb DistFiles/Linux/L10NSharp.pdb
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt324/phonologyAssistant.tcbuildtag/SIL.Core.dll DistFiles/Linux/SIL.Core.dll
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt324/phonologyAssistant.tcbuildtag/SIL.Core.dll.config DistFiles/Linux/SIL.Core.dll.config
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt324/phonologyAssistant.tcbuildtag/SIL.Core.pdb DistFiles/Linux/SIL.Core.pdb
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt324/phonologyAssistant.tcbuildtag/SIL.Windows.Forms.dll DistFiles/Linux/SIL.Windows.Forms.dll
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt324/phonologyAssistant.tcbuildtag/SIL.Windows.Forms.dll.config DistFiles/Linux/SIL.Windows.Forms.dll.config
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt324/phonologyAssistant.tcbuildtag/SIL.Windows.Forms.pdb DistFiles/Linux/SIL.Windows.Forms.pdb
		# End of script
elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]]; then
        echo "windows"
		mkdir -p ../Installer/
		mkdir -p DistFiles/Windows

		# download artifact dependencies
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/phonologyAssistant.tcbuildtag/L10NSharp.dll DistFiles/Windows/L10NSharp.dll
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/phonologyAssistant.tcbuildtag/L10NSharp.pdb DistFiles/Windows/L10NSharp.pdb
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/phonologyAssistant.tcbuildtag/SIL.Core.dll DistFiles/Windows/SIL.Core.dll
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/phonologyAssistant.tcbuildtag/SIL.Core.dll.config DistFiles/Windows/SIL.Core.dll.config
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/phonologyAssistant.tcbuildtag/SIL.Core.pdb DistFiles/Windows/SIL.Core.pdb
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/phonologyAssistant.tcbuildtag/SIL.Windows.Forms.dll DistFiles/Windows/SIL.Windows.Forms.dll
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/phonologyAssistant.tcbuildtag/SIL.Windows.Forms.dll.config DistFiles/Windows/SIL.Windows.Forms.dll.config
		copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/phonologyAssistant.tcbuildtag/SIL.Windows.Forms.pdb DistFiles/Windows/SIL.Windows.Forms.pdb
		# End of script
else
echo "Unknown."
fi
#read t
