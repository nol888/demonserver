-- +----------------------------------------------------------------------------+
-- |	Demon - dAmn Emulator												    |
-- |============================================================================|
-- |	Copyright © 2008 Nol888												    |
-- |============================================================================|
-- |	This file is part of Demon.											    |
-- |																		    |
-- |	Demon is free software: you can redistribute it and/or modify		    |
-- |	it under the terms of the GNU Affero General Public License as		    |
-- |	published by the Free Software Foundation, either version 3 of the	    |
-- |	License, or (at your option) any later version.						    |
-- |																		    |
-- |	This program is distributed in the hope that it will be useful,		    |
-- |	but WITHOUT ANY WARRANTY; without even the implied warranty of		    |
-- |	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the		    |
-- |	GNU Affero General Public License for more details.					    |
-- |																		    |
-- |	You should have received a copy of the GNU Affero General Public License|
-- |	along with this program.  If not, see <http://www.gnu.org/licenses/>.	|
-- |																			|
-- |============================================================================|
-- |	> $Date$
-- |	> $Revision$
-- |	> $Author$
-- +----------------------------------------------------------------------------+

-- phpMyAdmin SQL Dump
-- version 3.2.0-dev
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Feb 21, 2009 at 11:57 PM
-- Server version: 5.0.67
-- PHP Version: 5.2.8

SET FOREIGN_KEY_CHECKS=0;

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

SET AUTOCOMMIT=0;
START TRANSACTION;

--
-- Database: `demonserver`
--

DROP DATABASE IF EXISTS `demonserver`;
CREATE DATABASE `demonserver` /*!40100 DEFAULT CHARACTER SET utf8 COLLATE utf8_bin */;
USE `demonserver`;

-- --------------------------------------------------------

--
-- Table structure for table `chatrooms`
--

DROP TABLE IF EXISTS `chatrooms`;
CREATE TABLE IF NOT EXISTS `chatrooms` (
  `chatroom_id` int(6) unsigned NOT NULL auto_increment COMMENT 'The ID of the chatroom.',
  `chatroom_creator_id` int(6) unsigned NOT NULL COMMENT 'The ID of the creator of the chatroom.',
  `chatroom_name` varchar(30) collate utf8_bin NOT NULL COMMENT 'The name of the chatroom.',
  `chatroom_title` int(6) unsigned NOT NULL COMMENT 'The current title of the chatroom.',
  `chatroom_topic` int(6) unsigned NOT NULL COMMENT 'The current topic of the chatroom.',
  `room_privs` int(3) unsigned NOT NULL default '127' COMMENT 'The combined flag that stores the global privs of the room.  See DemonServer.DAmnRoom.Privs for more info.',
  PRIMARY KEY  (`chatroom_id`),
  UNIQUE KEY `chatroom_name` (`chatroom_name`),
  KEY `roomcreatorid` (`chatroom_creator_id`),
  KEY `chatroomtitleref` (`chatroom_title`),
  KEY `chatroomtopicref` (`chatroom_topic`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin ROW_FORMAT=COMPACT COMMENT='Stores core data about all chatrooms existing on the server.' AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Table structure for table `privclasses`
--

DROP TABLE IF EXISTS `privclasses`;
CREATE TABLE IF NOT EXISTS `privclasses` (
  `privclass_id` int(8) unsigned NOT NULL auto_increment COMMENT 'The ID of the privclass.',
  `privclass_name` varchar(255) collate utf8_bin NOT NULL COMMENT 'The name of the privclass.',
  `chatroom_id` int(6) unsigned NOT NULL COMMENT 'The room to which the privclass belongs.',
  `order` smallint(2) unsigned NOT NULL COMMENT 'The chatroom order of the privclass.',
  `admin` bit NOT NULL default 0 COMMENT 'Admin privs?',
  `kick` bit NOT NULL default 0 COMMENT 'Kick privs?',
  `join` bit NOT NULL default 1 COMMENT 'Join privs?',
  `msg` bit NOT NULL default 1 COMMENT 'Message privs?',
  `topic` bit NOT NULL default 0 COMMENT 'Topic privs?',
  `title` bit NOT NULL default 0 COMMENT 'Title privs?',
  `shownotice` bit NOT NULL default 1 COMMENT 'Show notices upon join/part?',
  `promote` smallint(2) unsigned NOT NULL default '0' COMMENT 'Promote privs.',
  `demote` smallint(2) unsigned NOT NULL default '0' COMMENT 'Demote privs.',
  `images` mediumint(3) NOT NULL default '0' COMMENT 'Image privs. -1 for unlimited, 0 for none, [number] max.',
  `smilies` mediumint(3) NOT NULL default '0' COMMENT 'Smilie privs. -1 for unlimited, 0 for none, [number] max.',
  `emoticons` mediumint(3) NOT NULL default '0' COMMENT 'Emoticon privs. -1 for unlimited, 0 for none, [number] max.',
  `thumbs` mediumint(3) NOT NULL default '0' COMMENT 'Thumb privs. -1 for unlimited, 0 for none, [number] max.',
  `avatars` mediumint(3) NOT NULL default '0' COMMENT 'Avatar privs. -1 for unlimited, 0 for none, [number] max.',
  `websites` mediumint(3) NOT NULL default '0' COMMENT 'Website privs. -1 for unlimited, 0 for none, [number] max.',
  `objects` mediumint(3) NOT NULL default '0' COMMENT 'Object privs. -1 for unlimited, 0 for none, [number] max.',
  `default` bit NOT NULL default 0 COMMENT 'Default Privclass?',
  PRIMARY KEY  (`privclass_id`),
  KEY `chatroomidref` (`chatroom_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Privclass definition table.' AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Table structure for table `privclasses_entries`
--

DROP TABLE IF EXISTS `privclasses_entries`;
CREATE TABLE IF NOT EXISTS `privclasses_entries` (
  `entry_id` int(9) unsigned NOT NULL auto_increment COMMENT 'The ID of the privclass entry.',
  `entry_privclass_id` int(8) unsigned NOT NULL COMMENT 'ID of the privclass the entry belongs to.',
  `entry_user_id` int(6) unsigned NOT NULL COMMENT 'The user that is being assigned to the privclass.',
  PRIMARY KEY  (`entry_id`),
  UNIQUE KEY `useridref` (`entry_user_id`),
  KEY `privclassidref` (`entry_privclass_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Contains user entries for privclasses.' AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Table structure for table `topictitles`
--

DROP TABLE IF EXISTS `topictitles`;
CREATE TABLE IF NOT EXISTS `topictitles` (
  `id` int(6) unsigned NOT NULL auto_increment COMMENT 'The ID of the topic/title.',
  `text` longtext collate utf8_bin NOT NULL COMMENT 'Main body content.',
  `user_set` int(6) unsigned NOT NULL COMMENT 'User ID of the settor.',
  `time_set` timestamp NOT NULL default CURRENT_TIMESTAMP COMMENT 'Time the topic/title was set.',
  PRIMARY KEY  (`id`),
  KEY `usersetref` (`user_set`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Holds the topics/titles used in the chatroom.' AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
  `user_id` int(6) unsigned NOT NULL auto_increment COMMENT 'User''s ID.',
  `user_name` varchar(30) collate utf8_bin NOT NULL COMMENT 'Username',
  `password_hash` char(128) collate utf8_bin NOT NULL COMMENT 'SHA-512 hash represented as a hexidecimal string.',
  `password_salt` char(6) collate utf8_bin NOT NULL COMMENT 'Password salt.',
  `authtoken` char(32) collate utf8_bin NOT NULL COMMENT 'The user''s current authtoken.',
  `gpc` tinyint(1) NOT NULL COMMENT 'The GPC of the user.  -1 is banned, 0 is guest, 1 is admin.',
  `user_realname` varchar(100) collate utf8_bin NOT NULL default '' COMMENT 'The "real name" of the user.',
  `user_dtype` varchar(100) collate utf8_bin NOT NULL default 'Deviously Deviant' COMMENT 'The deviant type of the deviant.',
  `user_symbol` char(1) collate utf8_bin NOT NULL default '~' COMMENT 'The usersymbol of the deviant.',
  PRIMARY KEY  (`user_id`),
  UNIQUE KEY `user_name_2` (`user_name`),
  KEY `user_name` (`user_name`,`user_id`,`authtoken`,`password_hash`,`password_salt`),
  KEY `id_lookup` (`user_id`,`gpc`,`user_realname`,`user_dtype`,`user_symbol`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin ROW_FORMAT=COMPACT COMMENT='Stores user information.  Used for auth via pass/token.' AUTO_INCREMENT=1 ;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `chatrooms`
--
ALTER TABLE `chatrooms`
  ADD CONSTRAINT `chatrooms_ibfk_1` FOREIGN KEY (`chatroom_title`) REFERENCES `topictitles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `chatrooms_ibfk_2` FOREIGN KEY (`chatroom_topic`) REFERENCES `topictitles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `chatrooms_ibfk_3` FOREIGN KEY (`chatroom_creator_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `privclasses`
--
ALTER TABLE `privclasses`
  ADD CONSTRAINT `privclasses_ibfk_1` FOREIGN KEY (`chatroom_id`) REFERENCES `chatrooms` (`chatroom_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `privclasses_entries`
--
ALTER TABLE `privclasses_entries`
  ADD CONSTRAINT `privclasses_entries_ibfk_1` FOREIGN KEY (`entry_privclass_id`) REFERENCES `privclasses` (`chatroom_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `privclasses_entries_ibfk_2` FOREIGN KEY (`entry_user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `topictitles`
--
ALTER TABLE `topictitles`
  ADD CONSTRAINT `usersetref` FOREIGN KEY (`user_set`) REFERENCES `users` (`user_id`) ON UPDATE CASCADE;

SET FOREIGN_KEY_CHECKS=1;

COMMIT;
