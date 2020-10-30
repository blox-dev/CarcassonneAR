# AR board game STATE OF THE ART


# Table of Contents
1. [Our project](#Our-project)
2. [Motivation](#Motivation)
3. [The board game](#The-board-game)
4. [History of AR](#History-of-AR)
5. [What others do](#What-others-do)
    1. [Tendary](#Tendary) 
    2. [AR Works](#AR-Works)
    3. [Tilt Five](#Tilt-Five)
    4. [Unicorn Games: The Explorers](#Unicorn-Games:-The-Explorers)
    5. [Oracles Game: Civil War](#Oracles-Game:-Civil-War)
    6. [Live game board](#Live-game-board)
    7. [Spatial: Holographic AR Tabletop Gaming](#Spatial:-Holographic-AR-Tabletop-Gaming)
6. [What we will do in this project](#What-we-will-do-in-this-project)
7. [Used methods](#Used-methods)
8. [Libraries for AR](#Libraries-for-AR)
    1. [Free options](#Free-options)
    2. [Paid options](#Paid-options)
10. [Relevant links](#Relevant-links)
    1. [Research on AR Boardgames](#Research-on-AR-Boardgames)
## Our project

During this project we will build a board game with AR technologies using the [Vuforia SDK](https://developer.vuforia.com/) and Unity.

## Motivation

Some board games are based on pure strategy, but many contain an element of chance; and some are purely chance, with no element of skill. There are many varieties of board games. Rules can range from the very simple, to deeply complex.

The board game should be played on a flat surface, and its progress is shared between all players. 

We wanted to pick a multi-player game which is easy to learn, contains an element of chance, has moderately complex mechanics and is competitive. After some consideration, we chose:

## The board game

[Carcassonne](https://boardgamegeek.com/boardgame/822/carcassonne) is one of the most popular tile-placement game. The players may place a tile during their turn, and here comes the competitivity. You can always choose to expand your teritory or ruin your opponent's plan, and maybe strike new alliances or betray. While the game area increases with each placed tile, their owners can place meeples to claim points, having liberty to choose from many options, trying to maximize their advantage while also having to claim chances faster thain their competitors. 

![Carcassonne tiles](https://cf.geekdo-images.com/NojWLs0EBGQpdXq2q4-SSw__imagepagezoom/img/YdXQKSzrRzxzQkDaqP-ZXNQexiY=/fit-in/1200x900/filters:no_upscale():strip_icc()/pic265353.jpg)


![Carcassonne mid-game](https://cf.geekdo-images.com/7cEABqlgeBGNJuipYqzDRQ__imagepagezoom/img/CTBR1EuQEAl9ZTemDKff49U5jmk=/fit-in/1200x900/filters:no_upscale():strip_icc()/pic669244.jpg)

## History of AR

You can check here a [short history of AR](https://www.blippar.com/blog/2018/06/08/history-augmented-reality#:~:text=2000%3A%20AR%20Quake%20launched%20%2D%20the%20first%20AR%20game.) but in this document we will focus on AR board games.
You can also check this [article](https://www.interaction-design.org/literature/article/augmented-reality-the-past-the-present-and-the-future).

## What others do

We will first list some of the games out there to see what is the current industry standard and after that explain what we want to achieve.

### Tendary 

[Tendary](https://tendary.net/team-en) claims to be the first AR board game. It was released on December 2018.

![tendary_picture](https://pbs.twimg.com/media/Dd0iyNTVwAAK9_L.jpg)

Although Wikipedia claims one of the oldest AR game is [Cybergeneration](https://en.wikipedia.org/wiki/Cybergeneration).

### AR Works

Even though Tendary says it was the first board game in AR, board-game-like applications were developed before them. AR Works is a company that develops VR and AR app support. They have built medical and industrial AR applications, and in 2017 they made a game that you could play on a huge board outside: the Milka game.

![milka1](https://www.arworks.com/en/wp-content/uploads/2017/05/IMG_0424-1030x686.jpg)
![milka2](https://www.arworks.com/en/wp-content/uploads/2017/05/milkakiemelt.jpg)

You can check out AR works [here](http://www.arworks.com/en/our-work/).

### Tilt Five

Tilt is a new project that allows users to play in Augumented Reality. They provide a package of 3 items: glasses, an AR wand, and a board. It works with special glasses that have two small projectors which overlay an image over the lenses. The board is the place all the games are rendered onto. The wand can interact with the AR objects or can be used to point in the virtual scene.

![tiltfive1](https://o.aolcdn.com/images/dims?quality=95&image_uri=https%3A%2F%2Fs.yimg.com%2Fuu%2Fapi%2Fres%2F1.2%2Ff6tgLqU84KCnRA5iDWAYNw--%7EB%2FaD0xMDY3O3c9MTYwMDthcHBpZD15dGFjaHlvbg--%2Fhttps%3A%2F%2Fo.aolcdn.com%2Fimages%2Fdims%3Fcrop%3D1600%252C1067%252C0%252C0%26quality%3D85%26format%3Djpg%26resize%3D1600%252C1067%26image_uri%3Dhttps%253A%252F%252Fs.yimg.com%252Fos%252Fcreatr-uploaded-images%252F2019-09%252F6445dce0-de79-11e9-b8ff-6a75b8973002%26client%3Da1acac3e1b3290917d92%26signature%3D1ed36aeb3611a4d5e55a56baf1f0a0dbd3c33a79&client=amp-blogside-v2&signature=d3598aa90bc2420f98ec62ced7a530f58c5c4b26)

Tilt five was kickstarted but is currently only available for preorder.

### Unicorn Games: The Explorers

Unicorn Games have launched a [series](https://unicorngames.co/case-studies/case-explorers/) of three board games which encompass both traditional and augmented components using a mobile application that uses video games and card scanning. 

![The Explorers](https://unicorngames.co/wp-content/uploads/2017/11/06.jpg)

The player uses real-world dices and pawns to move across the gameboard and several condition may prompt the user to scan treasure chests with their camera or play mini-games to gain points.


### Oracles Game: Civil War

![CivilWar](https://kick.agency/news/wp-content/uploads/2018/11/Oracles-gra.jpg)

Oracles Game is a team oriented strategy board game with augmented reality feature, which uses Vuforia to scan targets and animate chests, monsters and other game elements with which you can interact using the mobile app or making real-life moves.

![CivilWarVuforia](https://cf.geekdo-images.com/XYVJXsydVP2Isvf908fsqw__imagepage/img/tN4iS6piQwfsrhj_czdr2JXKEgk=/fit-in/900x600/filters:no_upscale():strip_icc()/pic4411473.jpg)

### Live game board

An example business making AR games is [this](http://www.livegameboard.com/) company. They sell the board on which you can play the game.


### Spatial: Holographic AR Tabletop Gaming

![Spatial](https://s3-us-west-1.amazonaws.com/comingsoon-tech/project/carousel/Untitled-3_0000_Spatial-JStrutz-052418-0169-1_180806_113347.jpg)

This company started a kickstarter project that featured a device through which you can see the 3d world of the game and also tokens that can be recognized by the software. The project was funded but after that we didn't find any more news.

## What we will do in this project

Our goal is to create an interactable AR board game that can be played on mobile. 

The game we are trying to build has a particular unique mechanic: the board expands with the game. So far most AR boardgames have had a limited space in which the game takes place. This means the users will most likely have a wider play area then most board games.

We will try to research one more topic in our project. We could make our game interactable with hand gestures. Gesture detection libraries are still mostly in development state, and they are quite heavy on CPU usage. We have not reached a point where one of these libraries can provide out-of-the-box functionality for gesture recognition - ARFoundation and Vuforia for example, two of the most known frameworks for developing AR Unity games do not have this functionality (yet). Is it possible to deploy an app that has both multiplayer and a module for hand recognition - feasible on mobile?

## Used methods

We will try to detect a surface or a Vuforia target to set the initial piece of the board. During their turn, players will interact with the game from their screens (or if we succeed - with gestures by using [MediaPipe](#MediaPipe) - see below). The play mode is online multiplayer - we will have to create a room creation/entering interface. Networking will be handled by an api with an existing Unity package like Photon.

Throught our project we will use free resources, given that this is a project from which we should focus on learning about AR and VR. We will assess the current state of open-source-ness in AR game development by the end of this project.

## Glasses

AR Headsets and Glasses are costly, but we have bought some cardboard glasses from [Aryzon](https://www.aryzon.com/) and depending if their SDK is working or not, we could try bringing the game to the Aryzon games experience.

## Libraries for AR

### Free options

- #### Vuforia

Vuforia is an augmented reality SDK that enables businesses and app developers to quickly spin-up high fidelity, mobile-centric, immersive AR experiences. The Vuforia SDK leverages computer vision technology to identify and track image targets and 3D objects in real-time.

- #### ARKit (Apple sdk)

The ARKit SDK functions in the same way as most AR SDK’s function, by enabling digital information and 3D objects to be blended with the real world but offers largely unparalleled accessibility in terms of the number of existing devices that it supports. However, ARKit can be only run on any device equipped with an Apple A9, A10, or A11 processor.


- #### ARCore (Google sdk)

ARCore is Google’s proprietary augmented reality SDK. Similar to ARKit, it enables brands and developers to get AR apps up and running on compatible Google smartphones and tablets. One of the most notable features of ARCore is that it also supports iOS-enabled devices and gives developers unparalleled access to users across both platforms.

- #### EasyAR

The EasyAR SDK is available to businesses and developers across two-tiered pricing packages: EasyAR SDK Basic and EasyAR SDK Pro.

- #### MediaPipe

MediaPipe offers cross-platform, customizable ML solutions for live and streaming media. ML solutions in MediaPipe include: face detection, face mesh, hair segmentation, object detection, box tracking, instant motion tracking and many more.

![mediap](https://www.slashgear.com/wp-content/uploads/2019/08/google-mediapipe.jpg)

- #### Photon Engine

Photon Unity Networking (PUN) re-implements and enhances the features of Unity’s built-in networking. Under the hood, it uses Photon’s features to communicate and match players. The API is very similar to Unity’s. Developers with prior networking experience in Unity will feel at home immediately. An automatic converter assists you porting existing multiplayer projects.

### Paid options

- #### Wikitude (lowest tier = 2490 €)

The Wikitude SDK includes functionality such as 3D model rendering, location-based AR, and video overlay. Moreover it uses SLAM technology (simultaneous localization and mapping), which facilitates seamless object tracking and recognition alongside markerless instantaneous tracking.


## Relevant links

### Research on AR Boardgames

Some articles delve into the challenges of creating AR games and describe the implications of new technologies:

[Designing_Augmented_Reality_Board_Games_The_BattleBoard_3D_experience.pdf](https://www.researchgate.net/publication/249849714_Designing_Augmented_Reality_Board_Games_The_BattleBoard_3D_experience)

[Augmented_reality_for_board_games.pdf](https://www.researchgate.net/publication/224197774_Augmented_reality_for_board_games)
