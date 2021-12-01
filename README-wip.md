# TwoStageFileTransfer
 
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
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/knightofnet/TwoStageFileTransfer">
    <img src="images/logo.png" alt="Logo" width="80" height="80">
  </a>

<h3 align="center">Two-stage File Transfer</h3>

  <p align="center">
    TSFT aims to transfer a file from one computer to another, passing through a common shared directory whose disk space does not allow intermediate copies. On the source computer, the file will be split while the target computer will reassemble the file in the target folder. 
    <br />
    <a href="https://github.com/knightofnet/TwoStageFileTransfer"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/knightofnet/TwoStageFileTransfer">View Demo</a>
    ·
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
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

[![Product Name Screen Shot][product-screenshot]](https://example.com)

TSFT aims to transfer a file from one computer to another, passing through a common shared directory whose disk space does not allow intermediate copies. On the source computer, the file will be split while the target computer will reassemble the file in the target folder. 

I started this project because in my company, I work on a computer but also on a remote desktop. In this configuration, a network drive shared between the two machines allows data and file exchanges. The problem is that this network drive (shared) is limited in size: it is therefore not possible to transfer large files between the two machines in a simple way (or else, by going through a multi-part archive and then exchanging a few parts, but this is... laborious for an operation that should be simple in the beginning).

That's what this tool was born from.

<p align="right">(<a href="#top">back to top</a>)</p>



### Built With

* [Next.js](https://nextjs.org/)
* [React.js](https://reactjs.org/)
* [Vue.js](https://vuejs.org/)
* [Angular](https://angular.io/)
* [Svelte](https://svelte.dev/)
* [Laravel](https://laravel.com)
* [Bootstrap](https://getbootstrap.com)
* [JQuery](https://jquery.com)

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

This is an example of how you may give instructions on setting up your project locally.
To get a local copy up and running follow these simple example steps.

### Prerequisites

This application runs on Microsoft Windows with .net Framework 4.5.2.

To test that you have the minimum version required, you can run this Powershell command:

1. Open Powershell buy typing ```powershell``` into command prompt, or start menu.
2. Write this 
```
(Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full").Release -ge 379893
```
and valid with return.

3. If you can see ```True```, then everything is OK. Else, download and install .net Framework 4.5.2 [here](https://www.microsoft.com/fr-fr/download/details.aspx?id=42642).


### Installation

1. Download latest release [here]().
2. Extract the archive in a folder.

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

Use this space to show useful examples of how a project can be used. Additional screenshots, code examples and demos work well in this space. You may also link to more resources.

_For more examples, please refer to the [Documentation](https://example.com)_

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



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Aryx - [@wolfaryx](https://twitter.com/wolfaryx) (wolfaryx [AT] gmail [DOT] com)

Project Link: [https://github.com/knightofnet/TwoStageFileTransfer](https://github.com/knightofnet/TwoStageFileTransfer)

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* The translation and the assistance to the translation were realized with DeepL [https://www.deepl.com/translator](https://www.deepl.com/translator)
* []()
* []()

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
