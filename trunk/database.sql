-- phpMyAdmin SQL Dump
-- version 3.1.0-dev
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Sep 27, 2008 at 11:04 AM
-- Server version: 5.1.22
-- PHP Version: 5.2.6

SET FOREIGN_KEY_CHECKS=0;

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

SET AUTOCOMMIT=0;
START TRANSACTION;

--
-- Database: `demonserver`
--

-- --------------------------------------------------------

--
-- Table structure for table `chatrooms`
--

DROP TABLE IF EXISTS `chatrooms`;
CREATE TABLE IF NOT EXISTS `chatrooms` (
  `chatroom_id` int(6) unsigned NOT NULL AUTO_INCREMENT COMMENT 'The ID of the chatroom.',
  `chatroom_creator_id` int(6) unsigned NOT NULL COMMENT 'The ID of the creator of the chatroom.',
  `chatroom_name` varchar(30) COLLATE utf8_bin NOT NULL COMMENT 'The name of the chatroom.',
  `chatroom_title` longtext COLLATE utf8_bin NOT NULL COMMENT 'The current title of the chatroom.',
  `chatroom_topic` longtext COLLATE utf8_bin NOT NULL COMMENT 'The current topic of the chatroom.',
  `privclass_collection` varchar(255) COLLATE utf8_bin NOT NULL COMMENT 'The comma seperated list of privclasses in the chatroom.',
  `room_privs` int(3) unsigned NOT NULL DEFAULT '127' COMMENT 'The combined flag that stores the global privs of the room.  See DemonServer.DAmnRoom.Privs for more info.',
  PRIMARY KEY (`chatroom_id`),
  UNIQUE KEY `chatroom_name` (`chatroom_name`),
  KEY `roomcreatorid` (`chatroom_creator_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COLLATE=utf8_bin ;

-- --------------------------------------------------------

--
-- Table structure for table `privclasses`
--

DROP TABLE IF EXISTS `privclasses`;
CREATE TABLE IF NOT EXISTS `privclasses` (
  `privclass_id` int(8) unsigned NOT NULL COMMENT 'The ID of the privclass.',
  `privclass_name` varchar(255) COLLATE utf8_bin NOT NULL COMMENT 'The name of the privclass.',
  `order` smallint(2) unsigned NOT NULL COMMENT 'The chatroom order of the privclass.',
  `admin` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Admin privs?',
  `kick` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Kick privs?',
  `join` tinyint(1) NOT NULL DEFAULT '1' COMMENT 'Join privs?',
  `msg` tinyint(1) NOT NULL DEFAULT '1' COMMENT 'Message privs?',
  `topic` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Topic privs?',
  `title` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Title privs?',
  `shownotice` tinyint(1) NOT NULL DEFAULT '1' COMMENT 'Show notices apon join/part?',
  `promote` smallint(2) unsigned NOT NULL DEFAULT '0' COMMENT 'Promote privs.',
  `demote` smallint(2) unsigned NOT NULL DEFAULT '0' COMMENT 'Demote privs.',
  `images` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Image privs. -1 for unlimited, 0 for none, [number] max.',
  `smilies` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Smilie privs. -1 for unlimited, 0 for none, [number] max.',
  `emoticons` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Emoticon privs. -1 for unlimited, 0 for none, [number] max.',
  `thumbs` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Thumb privs. -1 for unlimited, 0 for none, [number] max.',
  `avatars` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Avatar privs. -1 for unlimited, 0 for none, [number] max.',
  `websites` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Website privs. -1 for unlimited, 0 for none, [number] max.',
  `objects` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Object privs. -1 for unlimited, 0 for none, [number] max.',
  `default` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Default Privclass?',
  PRIMARY KEY (`privclass_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Privclass definition table.';

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
  `user_id` int(6) unsigned NOT NULL AUTO_INCREMENT COMMENT 'User''s ID.',
  `user_name` varchar(30) COLLATE utf8_bin NOT NULL COMMENT 'Username',
  `password_hash` char(128) COLLATE utf8_bin NOT NULL COMMENT 'SHA-512 hash represented as a hexidecimal string.',
  `password_salt` char(6) COLLATE utf8_bin NOT NULL COMMENT 'Password salt.',
  `authtoken` char(32) COLLATE utf8_bin NOT NULL COMMENT 'The user''s current authtoken.',
  `gpc` tinyint(1) NOT NULL COMMENT 'The GPC of the user.  -1 is banned, 0 is guest, 1 is admin.',
  `user_realname` varchar(100) COLLATE utf8_bin NOT NULL DEFAULT '' COMMENT 'The "real name" of the user.',
  `user_dtype` varchar(100) COLLATE utf8_bin NOT NULL DEFAULT 'Deviously Deviant' COMMENT 'The deviant type of the deviant.',
  `user_symbol` char(1) COLLATE utf8_bin NOT NULL DEFAULT '~' COMMENT 'The usersymbol of the deviant.',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `user_name_2` (`user_name`),
  KEY `user_name` (`user_name`,`user_id`,`authtoken`,`password_hash`,`password_salt`),
  KEY `id_lookup` (`user_id`,`gpc`,`user_realname`,`user_dtype`,`user_symbol`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COLLATE=utf8_bin ;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `chatrooms`
--
ALTER TABLE `chatrooms`
  ADD CONSTRAINT `roomcreatorid` FOREIGN KEY (`chatroom_creator_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE;

SET FOREIGN_KEY_CHECKS=1;

COMMIT;
