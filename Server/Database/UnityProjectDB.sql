-- MySQL dump 10.13  Distrib 8.0.34, for Win64 (x86_64)
--
-- Host: localhost    Database: finalprojectdb
-- ------------------------------------------------------
-- Server version	8.1.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `lobbies`
--

DROP TABLE IF EXISTS `lobbies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `lobbies` (
  `LobbyID` int NOT NULL AUTO_INCREMENT,
  `IsGameActive` tinyint DEFAULT '0',
  `Player1ID` int DEFAULT NULL,
  `Player2ID` int DEFAULT NULL,
  PRIMARY KEY (`LobbyID`),
  UNIQUE KEY `LobbyID_UNIQUE` (`LobbyID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='This table holds all active and inactice match slots';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `lobbies`
--

LOCK TABLES `lobbies` WRITE;
/*!40000 ALTER TABLE `lobbies` DISABLE KEYS */;
/*!40000 ALTER TABLE `lobbies` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `players`
--

DROP TABLE IF EXISTS `players`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `players` (
  `PlayerID` int NOT NULL AUTO_INCREMENT,
  `PlayerToken` int DEFAULT NULL,
  `PlayerName` varchar(64) DEFAULT 'Player',
  `LobbyNumber` int DEFAULT '0' COMMENT 'Points to the lobby the player is in.\nNULL if not in a lobby.',
  `PlayerStatus` int NOT NULL DEFAULT '0' COMMENT 'Represents the player''s current location in game and in the database.\\n0 = Idle.\\n1 = Queue.\\n2 = Lobby.',
  PRIMARY KEY (`PlayerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Holds information on the currently connected players';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `players`
--

LOCK TABLES `players` WRITE;
/*!40000 ALTER TABLE `players` DISABLE KEYS */;
/*!40000 ALTER TABLE `players` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `questions`
--

DROP TABLE IF EXISTS `questions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `questions` (
  `QuestionID` int NOT NULL AUTO_INCREMENT COMMENT 'The ID of the question',
  `QuestionText` varchar(128) NOT NULL COMMENT 'The question itself, in text form.',
  `CorrectAnswer` int NOT NULL COMMENT 'Describes which answer (Answer1 through Answer4) is the correct one.',
  `Answer1` varchar(64) NOT NULL,
  `Answer2` varchar(64) NOT NULL,
  `Answer3` varchar(64) NOT NULL,
  `Answer4` varchar(64) NOT NULL,
  PRIMARY KEY (`QuestionID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Each question holds both the question itself, and the answers';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `questions`
--

LOCK TABLES `questions` WRITE;
/*!40000 ALTER TABLE `questions` DISABLE KEYS */;
INSERT INTO `questions` VALUES (1,'Which company developed the first Call of Duty?',2,'Treyarch','Infinity Ward','Sledgehammer Games','Activision'),(2,'Which video game sold the most copies?',4,'Wii Sports','Tetris','GTA 5','Minecraft'),(3,'Which pro League of Legends team has won the most world championships?',1,'SKT 1','TSM','Team Liquid','Fnatics'),(4,'What was the very first video game ever created?',1,'Tennis for Two, 1958','Pong, 1970','Gun Fight, 1975','Jump Rope Game, 1951'),(5,'What is the quickest way to gain power in the Devil May Cry universe?',3,'Killing a powerful demon','Getting a magical weapon','Attempted suicide','Depression'),(6,'How many times has Captain America died and came back to life in the comics?',4,'0','1','5','11'),(7,'In the God of War games, why did Kratos murder all of Olympus?',3,'Zues killed his girlfriend','Athena made his best friend die in a war','Ares made him kill his own family','Ares destroyed his village'),(8,'Which of these characters DIDN\'T die at the end of Infinity War?',2,'Nick Fury','Rocket Racoon','Groot','Winter Soldier'),(9,'In League of Legend\'s lore, what is Jax\'s full name?',3,'Jaxuli Icathon','Jaxumundi Icathia Uk’nun','Saijax Cail-Rynx Kohari Icath’un','Jax'),(10,'In Ratchet and Clank - Rift Apart, what is the first weapon you are given at the start of the game?',2,'Negatron Collider','Burst Pistol','Headhunter','Steel Rifle');
/*!40000 ALTER TABLE `questions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `queue`
--

DROP TABLE IF EXISTS `queue`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `queue` (
  `PlayerID` int NOT NULL,
  `AcceptMatch` tinyint NOT NULL DEFAULT '0',
  PRIMARY KEY (`PlayerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `queue`
--

LOCK TABLES `queue` WRITE;
/*!40000 ALTER TABLE `queue` DISABLE KEYS */;
/*!40000 ALTER TABLE `queue` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `session stats`
--

DROP TABLE IF EXISTS `session stats`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `session stats` (
  `PlayerID` int NOT NULL,
  `LobbyID` int DEFAULT NULL,
  `Score` int DEFAULT '0',
  `CurrentQuestion` int DEFAULT '1',
  PRIMARY KEY (`PlayerID`),
  UNIQUE KEY `PlayerID_UNIQUE` (`PlayerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Holds all the players'' in game stats';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `session stats`
--

LOCK TABLES `session stats` WRITE;
/*!40000 ALTER TABLE `session stats` DISABLE KEYS */;
/*!40000 ALTER TABLE `session stats` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2023-10-16 17:35:00
