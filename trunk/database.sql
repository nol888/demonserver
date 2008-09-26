/*
+---------------------------------------------------------------------------+
|	Demon - dAmn Emulator													|
|===========================================================================|
|	Copyright © 2008 Nol888													|
|===========================================================================|
|	This file is part of Demon.												|
|																			|
|	Demon is free software: you can redistribute it and/or modify			|
|	it under the terms of the GNU Affero General Public License as			|
|	published by the Free Software Foundation, either version 3 of the		|
|	License, or (at your option) any later version.							|
|																			|
|	This program is distributed in the hope that it will be useful,			|
|	but WITHOUT ANY WARRANTY; without even the implied warranty of			|
|	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the			|
|	GNU Affero General Public License for more details.						|
|																			|
|	You should have received a copy of the GNU Affero General Public License|
|	along with this program.  If not, see <http://www.gnu.org/licenses/>.	|
|																			|
|===========================================================================|
|	> $Date$
|	> $Revision$
|	> $Author$
+---------------------------------------------------------------------------+
*/

-- phpMyAdmin SQL Dump
-- version 3.1.0-dev
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Sep 25, 2008 at 08:18 PM
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
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COLLATE=utf8_bin AUTO_INCREMENT=2 ;

-- --------------------------------------------------------

--
-- Table structure for table `privclasses`
--

DROP TABLE IF EXISTS `privclasses`;
CREATE TABLE IF NOT EXISTS `privclasses` (
  `privclass_id` int(8) unsigned NOT NULL COMMENT 'The ID of the privclass.',
  `privclass_name` varchar(255) COLLATE utf8_bin NOT NULL COMMENT 'The name of the privclass.',
  `order` smallint(2) unsigned NOT NULL COMMENT 'The chatroom order of the privclass.',
  `admin` bit(1) NOT NULL DEFAULT '\0' COMMENT 'Admin privs?',
  `kick` bit(1) NOT NULL DEFAULT '\0' COMMENT 'Kick privs?',
  `join` bit(1) NOT NULL DEFAULT '' COMMENT 'Join privs?',
  `msg` bit(1) NOT NULL DEFAULT '' COMMENT 'Message privs?',
  `topic` bit(1) NOT NULL DEFAULT '\0' COMMENT 'Topic privs?',
  `title` bit(1) NOT NULL DEFAULT '\0' COMMENT 'Title privs?',
  `shownotice` bit(1) NOT NULL DEFAULT '' COMMENT 'Show notices apon join/part?',
  `promote` smallint(2) unsigned NOT NULL DEFAULT '0' COMMENT 'Promote privs.',
  `demote` smallint(2) unsigned NOT NULL DEFAULT '0' COMMENT 'Demote privs.',
  `images` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Image privs. -1 for unlimited, 0 for none, [number] max.',
  `smilies` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Smilie privs. -1 for unlimited, 0 for none, [number] max.',
  `emoticons` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Emoticon privs. -1 for unlimited, 0 for none, [number] max.',
  `thumbs` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Thumb privs. -1 for unlimited, 0 for none, [number] max.',
  `avatars` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Avatar privs. -1 for unlimited, 0 for none, [number] max.',
  `websites` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Website privs. -1 for unlimited, 0 for none, [number] max.',
  `objects` mediumint(3) NOT NULL DEFAULT '0' COMMENT 'Object privs. -1 for unlimited, 0 for none, [number] max.',
  `default` bit(1) NOT NULL DEFAULT '\0' COMMENT 'Default Privclass?',
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
  PRIMARY KEY (`user_id`),
  KEY `user_name` (`user_name`,`user_id`,`authtoken`,`password_hash`,`password_salt`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COLLATE=utf8_bin AUTO_INCREMENT=2 ;

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
