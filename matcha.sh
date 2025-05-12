#!/bin/sh

# Handling of Matcha project

# Error handling
set -e # Exit immediately if a command exits with a non-zero status.
# set -u # Treat unset variables as an error when substituting.

# _____________________ Themes ______________________ #

RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
YELLOW='\033[0;33m'
RESET='\033[0m'
BOLD='\033[1m'

TITLE_COLOR='\033[32;49;1;4m'
AUTHOR_COLOR='\033[1;2;3;92m'
BASE_COLOR='\033[1;2;92m'

LOGO="
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡼⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠰⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣷⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢿⣷⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠈⢻⣿⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢆⠀⠀⠙⣿⣆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢧⠀⠀⠘⢿⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⡆⠀⠀⠘⣿⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⣿⠃⠀⠀⠀⣿⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⣿⡇⠀⠀⠀⠀⡿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡏⣀⣀⣀⠀⡜⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⣀⡤⠤⠒⠒⠋⠉⠉⠻⣧⠀⠀⠀⠈⠉⠁⠀⠀⠀⠢⢄⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⣾⣿⠀⠀⠀⠀⣀⣀⣀⣀⣤⣽⣦⣄⣀⣀⣀⣀⠀⠀⠀⠀⢹⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⣿⣿⣿⠷⠾⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠶⠚⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⢿⣿⡏⠀⠀⠀⠀⠀⠀⠈⠉⠉⠉⠉⠉⠉⠀⠀⠀⠀⠀⠀⠀⣸⠛⠻⣷⠀⠀
⠀⠀⠀⠀⠀⠸⣿⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠃⠀⢠⣿⠇⠀
⠀⠀⠀⠀⠀⠀⣹⣿⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣎⣠⣴⠿⠃⠀⠀
⢀⣠⠔⠒⠈⠉⠀⠹⣿⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⠾⠛⠛⠉⠒⠢⣄⠀
⣿⡁⠀⠀⠀⠀⠀⠀⠈⢻⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣾⡃⠀⠀⠀⠀⠀⠀⠀⡟
⠙⠻⣶⣀⠀⠀⠀⠀⠀⠀⠈⠙⠲⠦⣤⣄⣀⣀⣀⣤⣤⣾⣯⡵⠞⠋⠀⠀⠀⣀⠟⠀⠀⠀
⠀⠀⠀⠉⠛⠻⠿⠿⠶⠶⠤⠤⠤⣄⣀⣀⣀⣀⣀⣀⣀⣀⡠⠤⠤⠤⠴⠖⠉⠀⠀⠀⠀⠀
"

LOGO_BASE="
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀  ⣀⣀⣀⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⣀⡤⠤⠒⠒⠋⠉⠉⠉ ⠀⠀⠀⠉⠉⠉⠙⠒⠒⠢⢄⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⣾⣿⠀⠀⠀⠀⣀⣀⣀⣀⣤⣴⣦⣄⣀⣀⣀⣀⠀⠀⠀⠀⢹⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⣿⣿⣿⠷⠾⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠶⠚⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⢿⣿⡏⠀⠀⠀⠀⠀⠀⠈⠉⠉⠉⠉⠉⠉⠀⠀⠀⠀⠀⠀⠀⣸⠛⠻⣷⠀⠀
⠀⠀⠀⠀⠀⠸⣿⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠃⠀⢠⣿⠇⠀
⠀⠀⠀⠀⠀⠀⣹⣿⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣎⣠⣴⠿⠃⠀⠀
⢀⣠⠔⠒⠈⠉⠀⠹⣿⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⠾⠛⠛⠉⠒⠢⣄⠀
⣿⡁⠀⠀⠀⠀⠀⠀⠈⢻⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣾⡃⠀⠀⠀⠀⠀⠀⠀⡟
⠙⠻⣶⣀⠀⠀⠀⠀⠀⠀⠈⠙⠲⠦⣤⣄⣀⣀⣀⣤⣤⣾⣯⡵⠞⠋⠀⠀⠀⣀⠟⠀⠀⠀
⠀⠀⠀⠉⠛⠻⠿⠿⠶⠶⠤⠤⠤⣄⣀⣀⣀⣀⣀⣀⣀⣀⡠⠤⠤⠤⠴⠖⠉⠀⠀⠀⠀
"

header () {
    echo "
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀  ⣀⣀⣀⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⠀⠀⣀⡤⠤⠒⠒⠋⠉⠉⠉ ⠀⠀⠀⠉⠉⠉⠙⠒⠒⠢⢄⠀⠀⠀⠀⠀⠀⠀ Project: ${TITLE_COLOR}${PROJECT_NAME}${RESET}
⠀⠀⠀⠀⠀⣾⣿⠀⠀⠀⠀⣀⣀⣀⣀⣤⣴⣦⣄⣀⣀⣀⣀⠀⠀⠀⠀⢹⠀⠀⠀⠀⠀⠀ Author: ${AUTHOR_COLOR}${AUTHORS}${RESET}
⠀⠀⠀⠀⠀⣿⣿⣿⠷⠾⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠶⠚⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⢿⣿⡏⠀⠀⠀⠀⠀⠀⠈⠉⠉⠉⠉⠉⠉⠀⠀⠀⠀⠀⠀⠀⣸⠛⠻⣷⠀⠀ Path: ${BASE_COLOR}${PROJECT_PATH}${RESET}
⠀⠀⠀⠀⠀⠸⣿⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠃⠀⢠⣿⠇⠀ Branch: ${BASE_COLOR}${GIT_BRANCH}${RESET}
⠀⠀⠀⠀⠀⠀⣹⣿⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣎⣠⣴⠿⠃⠀⠀
⢀⣠⠔⠒⠈⠉⠀⠹⣿⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⠾⠛⠛⠉⠒⠢⣄⠀ Version: ${BASE_COLOR}${VERSION}${RESET}
⣿⡁⠀⠀⠀⠀⠀⠀⠈⢻⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣾⡃⠀⠀⠀⠀⠀⠀⠀⡟
⠙⠻⣶⣀⠀⠀⠀⠀⠀⠀⠈⠙⠲⠦⣤⣄⣀⣀⣀⣤⣤⣾⣯⡵⠞⠋⠀⠀⠀⣀⠟⠀⠀⠀
⠀⠀⠀⠉⠛⠻⠿⠿⠶⠶⠤⠤⠤⣄⣀⣀⣀⣀⣀⣀⣀⣀⡠⠤⠤⠤⠴⠖⠉⠀⠀⠀⠀
"
}

# ____________________ Variables ____________________ #

# Variables
AUTHORS="rcutte, atonkopi"
PROJECT_NAME="MATCHA"
VERSION="1.0"
PROJECT_PATH="$(cd "$(dirname "$0")" && pwd)"
CLIENT_PATH="${PROJECT_PATH}/client"
SERVER_PATH="${PROJECT_PATH}/server"
DOCKER_COMPOSE_FILE="./docker-compose.yml"
GIT_BRANCH=$(git branch --show-current)

# Pids
SERVER_PID=null
CLIENT_PID=null

# Check if the project is already running
# if [ $(docker ps -a | grep matcha | grep Up | wc -l) -gt 0 ]; then
#     echo "Matcha is already running"
#     exit 1
# fi

# ____________________ Functions ____________________ #

# Log
log_msg () {
    local timestamp
    timestamp=$(date +"%Y-%m-%d %T")
    echo "${CYAN} [$timestamp] ${RESET} $1"
}

# Server launch
server_launch () {
    log_msg "Checking for the postgres container"
    # Check for postgres container to be up
    while [ $(docker ps -a | grep matcha-database | grep Up | wc -l) -eq 0 ]; do
        log_msg "Waiting for the postgres container to be up"
        sleep 5
    done
    log_msg "Postgres container is up"
    log_msg "Starting the server in the background"
    cd ${SERVER_PATH}
    npm run re > /dev/null 2>&1 &
    SERVER_PID=$!
    log_msg "Server started with PID: ${SERVER_PID}"
}

# Client launch
client_launch () {
    log_msg "Starting the client in the background"
    cd ${CLIENT_PATH}
    npm run start > /dev/null 2>&1 &
    CLIENT_PID=$!
    log_msg "Client started with PID: ${CLIENT_PID}"
}

# Main Logic
cleanup() {
    log_msg "Cleaning up..."
    if [ ! -z "$SERVER_PID" ] && [ "$SERVER_PID" != "null" ]; then
        kill $SERVER_PID 2>/dev/null
        log_msg "Server stopped (PID: $SERVER_PID)"
    fi
    if [ ! -z "$CLIENT_PID" ] && [ "$CLIENT_PID" != "null" ]; then
        kill $CLIENT_PID 2>/dev/null
        log_msg "Client stopped (PID: $CLIENT_PID)"
    fi
    docker compose -f ${DOCKER_COMPOSE_FILE} down
    log_msg "Docker containers stopped"
    exit 0
}

# Set up trap
trap cleanup 2 15

dev () {
    log_msg "Starting Matcha in development mode"
    log_msg "Starting the postgres container"
    docker compose -f ${DOCKER_COMPOSE_FILE} up --detach
    server_launch
    client_launch
    
    # Inform user about the trap to stop the services
    log_msg "Services running. Press Ctrl+C to stop"
    
    # Keep script running
    while true; do
        sleep 1
        # Check if processes are still running
        if ! kill -0 $SERVER_PID 2>/dev/null; then
            log_msg "Server process died"
            cleanup
        fi
        if ! kill -0 $CLIENT_PID 2>/dev/null; then
            log_msg "Client process died"
            cleanup
        fi
    done
}

# ____________________ Docker ____________________ #

# Check and start Docker services
check_docker_and_start() {
    # Check if Docker is installed
    if ! command -v docker >/dev/null 2>&1; then
        echo "${RED}Docker is not installed. Please install Docker to use this script.${RESET}"
        exit 1
    fi
    # Check if Docker is running
    if ! docker info >/dev/null 2>&1; then
        echo "${RED}Docker is not running. Please start Docker to use this script.${RESET}"
        exit 1
    fi
    # Check if Docker Compose Project is running
    if [ $(docker ps -a | grep matcha | grep Up | wc -l) -gt 0 ]; then
        echo "${RED}Matcha is already running. Please stop it before starting a new instance.${RESET}"
        exit 1
    fi
    # Launch Docker Compose
    echo "${GREEN}Starting Docker Compose...${RESET}"
    docker compose -f ${DOCKER_COMPOSE_FILE} up --detach --remove-orphans
    if [ $? -ne 0 ]; then
        echo "${RED}Failed to start Docker Compose. Please check the logs.${RESET}"
        exit 1
    fi
}

# ____________________ Arguments ___________________ #

usage() {
    echo "${TITLE_COLOR}Usage:${RESET} matcha.sh [OPTION]"
    echo ""
    echo "Options:"
    echo "  ${YELLOW}-h${RESET}    Show this help message"
    echo "  ${YELLOW}-d${RESET}    Start the project in development mode"
}

while getopts "hd" opt; do
    case ${opt} in
        h)
            header
            usage
            exit 0
            ;;
        d)
            dev ;;
        \?)
            usage
            exit 1
            ;;
    esac
done

# If no arguments are passed, show help and header
header
check_docker_and_start

exit 0
