# EncryptXML Example
A quick and very dirty example project for using EncryptXml to decrypt XML with x.509 certificates in Cloud Foundry.

## Prerequisites
You will need Visual Studio, and [the Cloud Foundry CLI](https://console.run.pivotal.io/tools) installed.

You will also need access to an instance of Cloud Foundry with the Windows stack enabled.  [Pivotal Cloud Foundry with DiegoWindows](https://network.pivotal.io/products/elastic-runtime) or perhaps an OSS deployment of Cloud Foundry with [BOSH deployed Windows Diego cells](https://github.com/cloudfoundry-incubator/bosh-windows-stemcell-builder) would be fine.

If you want to test out scaling up your app, you will also need a Redis server.  This example uses the [Redis for PCF tile](https://network.pivotal.io/products/p-redis) deployed in Pivotal Cloud Foundry.

## Certificate Creation
To get the example started, let's generate a new, self-signed X.509 Certificate for encryption.
```Powershell
$cert = New-SelfSignedCertificate -Type DocumentEncryptionCert -DnsName "TestEncrypt" -HashAlgorithm SHA256 -Provider "Microsoft Enhanced Cryptographic Provider v1.0" -CertStoreLocation Cert:\CurrentUser\My
$mypwd = ConvertTo-SecureString -String "password" -Force -AsPlainText
$cert | Export-PfxCertificate -FilePath "TestEncrypt.pfx" -Password $mypwd -Force
```

Remember the location you created the `TestEncrypt.pfx` certificate/key file, as we will upload it into the running application later on.

## Generate Encrypted XML
Now, we can use the same Powershell session you used to create the certificate to generate some encrypted XML:
```Powershell
$xmldoc = New-Object System.Xml.XmlDocument
$xmlDoc.LoadXml("<root><creditcard><number>12345</number><expiry>02/02/2002</expiry></creditcard></root>")
$elementToEncrypt = $xmldoc.GetElementsByTagName("creditcard")[0]
$eXml = New-Object System.Security.Cryptography.Xml.EncryptedXml
$edElement = $eXml.Encrypt($elementToEncrypt, $cert)
[System.Security.Cryptography.Xml.EncryptedXml]::ReplaceElement($elementToEncrypt, $edElement, $false)
$xmldoc.OuterXml
```
And the result of the last command should look something like the following (trying to decrypt this will fail because you don't have the same private key used with the X.509 cert):
```XML
<root>
    <EncryptedData Type="http://www.w3.org/2001/04/xmlenc#Element" xmlns="http://www.w3.org/2001/04/xmlenc#"><EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#aes256-cbc" />
        <KeyInfo xmlns="http://www.w3.org/2000/09/xmldsig#">
            <EncryptedKey xmlns="http://www.w3.org/2001/04/xmlenc#">
                <EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#rsa-1_5" />
                <KeyInfo xmlns="http://www.w3.org/2000/09/xmldsig#">
                    <X509Data>
                        <X509Certificate>MIIDFTCCAf2gAwIBAgIQPc1pjGy5W6dDc3hV83T0bTANBgkqhkiG9w0BAQsFADAWMRQwEgYDVQQDDAtUZXN0RW5jcnlwdDAeFw0xNjA5MjEwMzIyMTRaFw0xNzA5MjEwMzQyMTRaMBYxFDASBgNVBAMMC1Rlc3RFbmNyeXB0MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzKek5JS4jAcQmVDYHa02w9Aj0Zyhoml3yAlJ5X7sjv3QhwfgPq6hLltgsLXJHxLGWdus7pIKtLbuMO4HVnHZlQ+SN8dUmFvoA5tEQcy+mqG+Ne9NQSXNVXP7OnWD9RqWkyHLYiI6Qf7n4sHtz/sfwoj9WogCUYV5Qgg1ORhy1WMLtTgufj5aJDWXFxIu5ofPo7EjvcjaqnuMnjG3R0QOvzQgwfwgzh74Acp00SJBrULAyOt8FP9QC624X49+562BreZeqgIzXXjaOemRAd+jGbhoBr4ukQH72kFj8hC1IgC2XL03A+g3jYQKCClC1GyUiIbrx/hLuL233hsXJkKwuwIDAQABo18wXTAOBgNVHQ8BAf8EBAMCBDAwFAYDVR0lBA0wCwYJKwYBBAGCN1ABMBYGA1UdEQQPMA2CC1Rlc3RFbmNyeXB0MB0GA1UdDgQWBBTCdIoXfr5GxFJo3rn7rX/3w/nPcTANBgkqhkiG9w0BAQsFAAOCAQEAGSGBG5SWHBTDnkCPWkIfgbldO/rTfH4ZwAcpSipzcq7pnsfSr1aminynS54A3HYnZNMKYeJ6I3WZ2PTs6j7cTUag/hriHlffA5Tqfpx9Arkl5QlPbm3MkDXRf6X/A5at44pgxtk+3yHymFWaZ8InKnsf02I/7YrqrhR0kfE5UJxoBCBSJ404clIuPRl9Ihbh9x93gwIjDCSGnAlRYGv53dsatcw+qXopCn6YcjuLBbkxgc2t0F+mxXS99Inttc/w72cKK2oXeqH+qe0P7bSkgSzmI3TKBxCWjaZ1e+8G6nA4iOF03idRXcwRFgpx8Hy1NrnMzFrdJn59bpbiA8F6WQ==</X509Certificate>
                    </X509Data>
                </KeyInfo>
                <CipherData>
                    <CipherValue>kX/LPRc2H5D3G1Ey+OJz55RtKr1MYOTyfwHv8q8Q2vKQ/6R8wFBkqgvNqGKOpwGNDhnkdwoiue8uJTbMQyc/rMtlBQ8FCLzRWE5RhUN90/wuDVzxIRWhYm9cLggRZxrZlptC2pJYBa/BjuDgOUGklNYA7Cj4fzwZtCzKEK7B64KrQ4apNmxgyb2hQyLO30Lcq5amohYclGIMJrD2n+AMxjm15/rNQ6RAU4Due4l+Hsf9IoHAxDyt3Cb3C+6egpcKt6Equ3RYX096CXaKOCeKP1wuU8V8bjMhJQ+8iW0k/UA2DltvK2D6+oE072MmXs/x6NQRkDGA1kYcJGrFQzOAOA==</CipherValue>
                </CipherData>
            </EncryptedKey>
        </KeyInfo>
        <CipherData>
            <CipherValue>4zyd1q63wTIEMkSwPbi0c+jqiL1ea+IL48HMJdIKImV7ZTjQnVwtveEylCinxWf4yAfq73W9iNb0PGQZksfCVyxXGyXSN/v344ChVFG1bnBD9K/gRQ6xRzCF6rSE42Ec</CipherValue>
        </CipherData>
    </EncryptedData>
</root>
```

Keep this encrypted XML to use in the app when you start it.

## Publish and deploy
Next you need to publish the EncryptXmlExample web app and push it to Cloud Foundry.

You can publish the project to a file system target.  There is a pre-existing publish profile called "Local Disk" that you can use to create a Published version of the app suitable for pushing to Cloud Foundry.  This publish profile will create a directory at the Solution level called EncryptXmlExamplePublished.

After publishing, open up a command prompt or Powershell window and navigate to the EncryptXmlExamplePublished directory.  Then, we can push the app to Cloud Foundry:
```
PS C:\Users\grog\Documents\Visual Studio 2015\Projects\EncryptXmlExample\EncryptXmlExamplePublished> cf push 
Using manifest file C:\Users\grog\documents\visual studio 2015\Projects\EncryptXmlExample\EncryptXmlExamplePublished\manifest.yml

Using stack windows2012R2...
OK
Creating app EncryptXmlExample in org cdelashmutt-org / space development as cdelashmutt@pivotal.io...
OK
...(some output removed for clarity)...
0 of 1 instances running, 1 starting
1 of 1 instances running

App started
OK

App EncryptXmlExample was started using this command `..\tmp\lifecycle\WebAppServer.exe`

Showing health and status for app EncryptXmlExample in org cdelashmutt-org / space development as cdelashmutt@pivotal.io...
OK

requested state: started
instances: 1/1
usage: 512M x 1 instances
urls: encryptxmlexample.cf.test.internal
last uploaded: Thu Sep 22 02:37:48 UTC 2016
stack: unknown
buildpack: binary_buildpack

     state     since                    cpu      memory           disk          details
#0   running   2016-09-21 10:40:58 PM   133.8%   247.9M of 512M   18.7M of 1G
```

## Testing the app
Browse to the host name listed in the `urls:` line in the output from the `cf push` command.  You will be presented with the front page of the app.

First, load a PFX certificate/key combo into the application.  The front page presents a section for you to upload a PFX file and specify the symbolic name you wish to use for the PFX and the password used to decrypt the PFX file.

Next, select the certificate in the top section of the app front page you want to use for decryption, then paste the encrypted xml you generated earlier into the text area labeled "Encrypted Text".  Click the "Decrypt" button, and you should see the decrypted text of the original XML doc.

## Multiple App Instances
Following the instructions above, you will be able to decrypt XML using a single instance of your application in Cloud Foundry.  But what if you want to scale your app up?  You will need some way to store your certificate data in a service that isn't tied to any one Windows VM or app instance.

In this example app, we use Redis to store the PFX file data, and the password used to decrypt that data.  The app looks for the existence of a service bound to the application called "certstore", and uses the StackExchange Redis client to access that instance of Redis.

To create a Redis service instance using the Redis for PCF service, simply create an instance of the service in the space you want to deploy your app to:
```
PS C:\Users\grog\Documents\Visual Studio 2015\Projects\EncryptXmlExample\EncryptXmlExamplePublished> cf create-service p-redis shared-vm certstore
Creating service instance certstore in org cdelashmutt-org / space development as cdelashmutt@pivotal.io...
OK
```

Then we need to bind it to our application, and restart the app:
```
PS C:\Users\grog\Documents\Visual Studio 2015\Projects\EncryptXmlExample\EncryptXmlExamplePublished> cf bind-service EncryptXmlExample certstore
Binding service certstore to app EncryptXmlExample in org cdelashmutt-org / space development as cdelashmutt@pivotal.io...
OK
TIP: Use 'cf restage EncryptXmlExample' to ensure your env variable changes take effect
PS C:\Users\grog\Documents\Visual Studio 2015\Projects\EncryptXmlExample\EncryptXmlExamplePublished> cf restart EncryptXmlExample
```

Try out the application again to see if everything works.  At this point, we're still running 1 instance of the app, so let's scale it to 2.
```
PS C:\Users\grog\Documents\Visual Studio 2015\Projects\EncryptXmlExample\EncryptXmlExamplePublished> cf scale EncryptXmlExample -i 2
Binding service certstore to app EncryptXmlExample in org cdelashmutt-org / space development as cdelashmutt@pivotal.io...
OK
TIP: Use 'cf restage EncryptXmlExample' to ensure your env variable changes take effect
PS C:\Users\grog\Documents\Visual Studio 2015\Projects\EncryptXmlExample\EncryptXmlExamplePublished> cf restart EncryptXmlExample
Scaling app EncryptXmlExample in org cdelashmutt-org / space development as cdelashmutt@pivotal.io...
OK
```

Wait for your new application instance to start up and become healthy:
```
PS C:\Users\grog\Documents\Visual Studio 2015\Projects\EncryptXmlExample\EncryptXmlExamplePublished> cf app EncryptXmlExample
Showing health and status for app EncryptXmlExample in org cdelashmutt-org / space development as cdelashmutt@pivotal.io...
OK

requested state: started
instances: 2/2
usage: 512M x 2 instances
urls: encryptxmlexample.cf.test.internal
last uploaded: Thu Sep 22 04:30:52 UTC 2016
stack: windows2012R2
buildpack: binary_buildpack

     state     since                    cpu     memory           disk          details
#0   running   2016-09-22 12:31:55 AM   0.6%    165M of 512M     21.5M of 1G
#1   running   2016-09-22 12:34:13 AM   0.0%    215.2M of 512M   20.8M of 1G
```

And now, you can access your application, load your cert, and decrypt on any instance of your application!

## How does this work?
Basically the code loads the cert just before it is needed from Memory/Redis into the "Current User" cert store location, and the "My" store name.  This is safe as every app instance running on a Windows Diego cell in CF get's its own user generated for that instance at the time the container for that app instance is created.  This means that the "Current User" cert store will live only as long as the app is running, and will be destroyed if that container is destroyed.

Have a look at the EncryptXmlExample.Controllers.HomeController.DoDecrypt method for how the cert gets loaded "just in time".