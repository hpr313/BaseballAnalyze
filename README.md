# Baseball Analyze

## Overview
This is a simple analyzing tool for the data from [Baseball Savant](https://baseballsavant.mlb.com/statcast_search?hfPT=&hfAB=&hfGT=R%7C&hfPR=&hfZ=&hfStadium=&hfBBL=&hfNewZones=&hfPull=&hfC=&hfSea=2024%7C&hfSit=&player_type=pitcher&hfOuts=&hfOpponent=&pitcher_throws=&batter_stands=&hfSA=&game_date_gt=&game_date_lt=&hfMo=&hfTeam=&home_road=&hfRO=&position=&hfInfield=&hfOutfield=&hfInn=&hfBBT=&hfFlag=&metric_1=&group_by=name&min_pitches=0&min_results=0&min_pas=0&sort_col=pitches&player_event_sort=api_p_release_speed&sort_order=desc#results).

## User Guide
STEP 1. Download the project.

STEP 2. Open cmd or anaconda prompt, then link to this path

STEP 3. Type the following command:
     
    python Analyze.py kodai_senga.csv

STEP 4. If it turns out the following error,

    ModuleNotFoundError: No module named 'XXXXX'

such error can be solved by installing the corresponding package. Below is the example command:
    
     pip install XXXXX

## Data Scrapying
Data scrapying and transfering is written by csharp. Scrapying methods are in

    ScrapyStacast.cs

API for data communication is in

    StacastController.cs
