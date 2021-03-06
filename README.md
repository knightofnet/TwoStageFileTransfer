# Two-stage File Transfer
 
<div id="top"></div>
<!--
*** Thanks for checking out the Best-README-Template. If you have a suggestion
*** that would make this better, please fork the repo and create a pull request
*** or simply open an issue with the tag "enhancement".
*** Don't forget to give the project a star!
*** Thanks again! Now go create something AMAZING! :D
-->



<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
<!--[![MIT License][license-shield]][license-url]-->



<!-- PROJECT LOGO -->
<br />
<div align="center">
<!--
  <a href="https://github.com/knightofnet/TwoStageFileTransfer">
    <img src="images/logo.png" alt="Logo" width="80" height="80">
  </a>
-->
<h3 align="center">Two-stage File Transfer</h3>

  <p align="center">
    TSFT aims to transfer a file from one computer to another, passing through a common shared directory whose disk space does not allow direct intermediate copies. On the source computer, the file will be split while the target computer will reassemble the file in the target folder. 
    <br /><br />
   <b>Not perfect yet, but still in development.</b>
    <br /><br />
    <a href="https://github.com/knightofnet/TwoStageFileTransfer"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/knightofnet/TwoStageFileTransfer/issues">Report Bug</a>
    ·
    <a href="https://github.com/knightofnet/TwoStageFileTransfer/issues">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

TSFT aims to transfer a file from one computer to another, passing through a common shared directory whose disk space does not allow direct intermediate copies. On the source computer, the file will be split while the target computer will reassemble the file in the target folder. 

I started this project because in my company, I work on a computer but also on a remote desktop. In this configuration, a network drive shared between the two machines allows data and file exchanges. The problem is that this network drive (shared) is limited in size: it is therefore not possible to transfer large files between the two machines in a simple way (or else, by going through a multi-part archive and then exchanging a few parts, but this is... laborious for an operation that should be simple in the beginning).

That's what this tool was born from.

<p align="right">(<a href="#top">back to top</a>)</p>



### Built Using

* [AryxDevLibrary (by me)](https://www.nuget.org/packages/AryxDevLibrary/)


<p align="right">(<a href="#top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

This application runs on Microsoft Windows with .net Framework 4.5.2.

To test that you have the minimum version required, you can run this Powershell command:

1. Open Powershell by typing ```powershell``` into command prompt, or start menu.
2. Write this 
```
(Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full").Release -ge 379893
```
and valid with return.

3. If you can see ```True```, then everything is OK. Else, download and install .net Framework 4.5.2 [here](https://www.microsoft.com/fr-fr/download/details.aspx?id=42642).


### Installation

1. Download latest release [here](https://github.com/knightofnet/TwoStageFileTransfer/releases).
2. Extract the archive in a folder.

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

The main use of the application is through the command line and input parameters. Here is the list of usable parameters:

```
Syntax: tsft.exe OPTIONS
```

OPTIONS :
- ``-d``    Required. Transfer direction. Values : ``in``,``out`` (also : ``--direction``)
- ``-s``    Path to the source file. For the 'in' mode, the file to transfer. For the 'out' mode (i.e. second stage), it depends on the type of the transfert. For ``Windows`` type transfers, it can be the first file to be transferred (i.e. the folder containing this first file) or the TSFT file generated at the first level. When it is a transfer using a remote server (type ``FTP`` or ``SFTP``), the 'out' mode only accepts a TSFT file as a source file. (also : ``--source``)
- ``-t``    Path for the target. For the 'in' mode, the folder where to generate the transfer files: this can be a local Windows folder or on the local network, but also a file on an FTP server (uri starting with ``ftp://``) or an SFTP server (uri starting with ``sftp://host:port``). For the latter two cases, this must be in accordance with the -p parameter. For the 'out' mode, to the folder where the reconstructed file will be placed. (also : ``--target``)
- ``-b``    Buffer size. Default: 8192 (also : ``--buffer-size``)
- ``-m``    Maximum size that will be used by all the transfer files. For 'Windows' type transfers, if this parameter is not set, the maximum size will be calculated in relation to the remaining disk space (90% of the remaining space). For transfers using a remote server, the parameter is recommended. If it is omitted, the maximum size used is arbitrarily 20MB. (also : ``--maxdiskplace``)
- ``-c``    Maximum size of a transfer file (file with a .part extension). Default: size of the available space divided by 10, 50MB max, or the size of the source file. (also : ``--chunk``)
- ``-w``    Overwrite existing files. Default: none (also : ``--overwrite``)
- ``-k``    Does not delete part files after reading in 'out' mode. Allows to restart the redial process several times, but may prevent the first step from finishing if the maximum size allowed by the transfer is reached and therefore the program is waiting (also : ``--keep-part-files``)
- ``-p``    Protocol type used for the transfer. Can be ``Windows``, ``Ftp``, ``Sftp``. Default: ``Windows`` (also : ``--protocol``)
- ``-pu``    Username for connecting to the remote server protocol. Used when -p is set to Ftp or Sftp. If set, can override username sets in TSFT file. Default: None. (also : --protocol-username)
- ``-pp``    Password for connecting to the remote server protocol. Used when -p is set to Ftp or Sftp. If set, can override password sets in TSFT file. Default: None. (also : ``--protocol-password``)
- ``-pw``    In the 'in' mode, when credentials need to be used (e.g. with the FTP,SFTP protocols), include these data in the generated TSFT file. This way, in the 'out' mode, these data will not be requested again, only the passphrase will be (also : ``--tsft-with-credentials``)
- ``-ph``    The passphrase used to encrypt (with 'in' mode) or decrypt (with 'out' mode) the TSFT file. If this parameter is omitted, the passphrase will be randomly generated in 'in' mode (look at the console or logs); for 'out' mode, it will be requested. (also : ``--passphrase``)
- ``-pn``    Allows you to use the default passphrase. (also : ``--passphrase-none``)

This list is displayed when the program is invoked without parameters.

### When this program can help ?

This program is mainly useful in the context of an internal network (business or home), when it is necessary to transfer a file from one computer to another, passing through an temporary folder such in a third-party computer, in a NAS or in a box-modem with an internal disk that serves as a folder; in fact anything that can be used as a Windows folder. This folder can have a size constraint and this tool will be able to manage it.

The program works in **two asynchronous stages** :

- The first stage consists of executing the program on a source computer: the latter will then split the source file into small intermediate files which will then be placed in the temporary folder. When the disk is full in this folder the program waits for the second stage to consume the intermediate files to continue.

- The second step runs on the target computer: it will recreate the file on the target computer using the intermediate files. If intermediate files are still being created by the first step, the program will wait.



### Simple transfer

To transfer a file from one computer to another, through a temporary folder, use the following commands:

- First stage (direction ``in``) : 

On the source computer (decompose the file to the temporary folder), type this :
```
tsft.exe -d in -s "C:\Folder\MyFile.bin" -t "W:\tempAndSharedFolder\"
```
It will split ``MyFile.bin`` into some part files (named with suffix partX, where X starting from 0) whose size depends on the available disk space. 


- Second stage (direction ``out``) : 

On the target computer (re-compose the file from the temporary folder):
```
tsft.exe -d out -s "W:\tempAndSharedFolder\" -t "D:\FinalFolder\"
```

*Remarks :*

1. When you generate parts files with first stage (in mode), a tsft file is created by the program (generally with same name that source file). This file helps the seconde stage (out mode) to work and add a final SHA1 verification to ensure file's integrity. This tsft file can be source for second stage. Example :

```
tsft.exe -d out -s "W:\tempAndSharedFolder\MyFile.tsft" -t "D:\FinalFolder\"
```

2. Part-files's size depends on available disk space. By default, less than 10% remaining free-space by file, or 50 Mo max. This can be overrided by ``-c`` parameter.

3. If source or target parameters are missing, the user will be prompted to fill them in. Into the console if the program is run in commands prompt, or with classics windows if the program is runned with explorer (or by using a classic shortcut). By ommiting these parameters and simply passing tsft file as paramater, the direction ``out`` is automaticaly setted and you will be prompted to select targer folder for recomposing file.

4. As soon as the TSFT file is created (or as soon as the *.part0 file is created) in the shared folder, then it is possible to run the program for the second step.


<p align="right">(<a href="#top">back to top</a>)</p>


<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- LICENSE 
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>
-->


<!-- CONTACT -->
## Contact

Aryx - [@wolfaryx](https://twitter.com/wolfaryx) (wolfaryx [AT] gmail [DOT] com)

Project Link: [https://github.com/knightofnet/TwoStageFileTransfer](https://github.com/knightofnet/TwoStageFileTransfer)

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* For some methods or some help, thanks to :
  * Matthew Watson, [*"get free space on network share"*](https://social.msdn.microsoft.com/Forums/en-US/b7db7ec7-34a5-4ca6-89e7-947190c4e043/get-free-space-on-network-share?forum=csharpgeneral).
  * Simon Mourier, for his cool select folder dialog on this *StackOverFlow's* thread [*"How to use OpenFileDialog to select a folder?"*](https://stackoverflow.com/a/66187224).
  * Simon Mourier (*again - just realize it's the same person who, indirectly, helps me twice*), for his solution to get the calling process on this *StackOverFlow's* thread [*"How to get parent process in .NET in managed way"*](https://stackoverflow.com/a/3346055) by Abatishchev.
* The translation and the assistance to the translation were realized with DeepL [https://www.deepl.com/translator](https://www.deepl.com/translator)
* Template of this README.MD file available [here](https://github.com/othneildrew/Best-README-Template).

*I like to think that programming is like playing with legos: you assemble blocks to form algorithms, functions, classes. At the end, it gives a program! (... and then you just spend your time to make it even better, or you start from the beginning for another one!)*

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/knightofnet/TwoStageFileTransfer.svg?style=for-the-badge
[contributors-url]: https://github.com/knightofnet/TwoStageFileTransfer/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/knightofnet/TwoStageFileTransfer.svg?style=for-the-badge
[forks-url]: https://github.com/knightofnet/TwoStageFileTransfer/network/members
[stars-shield]: https://img.shields.io/github/stars/knightofnet/TwoStageFileTransfer.svg?style=for-the-badge
[stars-url]: https://github.com/knightofnet/TwoStageFileTransfer/stargazers
[issues-shield]: https://img.shields.io/github/issues/knightofnet/TwoStageFileTransfer.svg?style=for-the-badge
[issues-url]: https://github.com/knightofnet/TwoStageFileTransfer/issues
[license-shield]: https://img.shields.io/github/license/knightofnet/TwoStageFileTransfer.svg?style=for-the-badge
[license-url]: https://github.com/knightofnet/TwoStageFileTransfer/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/linkedin_username
[product-screenshot]: images/screenshot.png
