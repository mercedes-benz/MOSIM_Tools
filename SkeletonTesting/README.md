# Setup Guide
In order to setup the local repository, please first clone it and open a powershell in the cloned folder. 

Untrack the .gitignore file and add meta files to the ignore list: 
```
cd Assets/Scripts/MMIFramework/
git update-index --assume-unchanged .\.gitignore
Add-Content .\.gitignore "`n*.meta"
```

Untrack dll files and remove them from the local folders. In case these dlls are updated, they have to be updated in this repository. Unity cannot handle multiple instances of the same dll in its folders. 
```
git update-index --skip-worktree .\MMIAdapterCSharp\Thrift.dll .\MMIAdapterCSharp\Utf8Json.dll MMIAdapterUnity/UnityEngine.dll MMICSharp/System.Threading.Tasks.Extensions.dll MMICSharp/Thrift.dll MMICSharp/Utf8Json.dll MMIStandard/Thrift.dll  MMIUnity/UnityEngine.dll  packages/MathNet.Numerics.4.9.0/lib/net40/MathNet.Numerics.dll packages/MathNet.Numerics.4.9.0/lib/net461/MathNet.Numerics.dll packages/MathNet.Numerics.4.9.0/lib/netstandard1.3/MathNet.Numerics.dll packages/MathNet.Numerics.4.9.0/lib/netstandard2.0/MathNet.Numerics.dll packages/System.Numerics.Vectors.4.5.0/lib/net46/System.Numerics.Vectors.dll packages/System.Numerics.Vectors.4.5.0/lib/netstandard1.0/System.Numerics.Vectors.dll packages/System.Numerics.Vectors.4.5.0/lib/netstandard2.0/System.Numerics.Vectors.dll packages/System.Numerics.Vectors.4.5.0/lib/portable-net45+win8+wp8+wpa81/System.Numerics.Vectors.dll packages/System.Numerics.Vectors.4.5.0/ref/net45/System.Numerics.Vectors.dll packages/System.Numerics.Vectors.4.5.0/ref/net46/System.Numerics.Vectors.dll packages/System.Numerics.Vectors.4.5.0/ref/netstandard1.0/System.Numerics.Vectors.dll packages/System.Numerics.Vectors.4.5.0/ref/netstandard2.0/System.Numerics.Vectors.dll packages/System.Runtime.Numerics.4.3.0/lib/netcore50/System.Runtime.Numerics.dll packages/System.Runtime.Numerics.4.3.0/lib/netstandard1.3/System.Runtime.Numerics.dll packages/System.Runtime.Numerics.4.3.0/ref/netcore50/System.Runtime.Numerics.dll packages/System.Runtime.Numerics.4.3.0/ref/netstandard1.1/System.Runtime.Numerics.dll

Remove-Item * -recurse -include *.dll
```

There are multiple AssemblyInfo files, which are irrelevant for Unity as well. Unfollow and remove them from your local instance: 
```
git update-index --skip-worktree MMIAdapterCSharp/Properties/AssemblyInfo.cs MMIAdapterUnity/Properties/AssemblyInfo.cs MMICSharp/Properties/AssemblyInfo.cs MMIStandard/Properties/AssemblyInfo.cs MMIUnity/Properties/AssemblyInfo.cs UnitTests/IntermediateSkeletonTest.cs

Remove-Item * -recurse -include *AssemblyInfo.cs
Remove-Item * -recurse -include *IntermediateSkeletonTest.cs
```


For the new version, you have to remove the MMIAdapterUnity folder in Assets/Scripts/MMIFramework. 
cd to the folder. make sure, that you have a clean repository. Remove the folder. Afterwards run the following command from the git bash console:
```
git ls-files --deleted -z | git update-index --assume-unchanged -z --stdin
```