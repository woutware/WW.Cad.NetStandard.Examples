# WW.Cad.NetStandard.Examples
Contains .NET Core and UWP samples for the [WW.Cad.NetStandard library](https://www.woutware.com/ww.cad.netstandard).

For these applications to work you will need a trial license.
The MyAppKeyPair.snk linked in the projects are not present in the repository, 
you should generate your own strong name key and keep it private.

1. You can generate a strong name key with the following command in the Visual Studio command prompt:
    ```sn -k MyKeyPair.snk```

1. The next step is to extract the public key file from the strong name key (which is a key pair):
    ```sn -p MyKeyPair.snk MyPublicKey.snk```

1. Display the public key token for the public key: 	
    ```sn -t MyPublicKey.snk```

1. Go to the project properties Singing tab, and check the "Sign the assembly" checkbox, 
   and choose the strong name key you created.

1. Register and get your trial license from [https://www.woutware.com/SoftwareLicenses](https://www.woutware.com/SoftwareLicenses).
   Enter your strong name key public key token that you got at step 3.
