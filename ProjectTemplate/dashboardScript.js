// Chart variables
let pointsDoughnutContext;
let pointsDoughnut;
let lineChartContext;
let lineChart;
let satisfactionDoughnutContext;
let satisfactionDoughnut;
let radarChartContext;
let radarChart;

function initializeDashboard() {
    //Points Goal Completion Dashboard
    pointsDoughnutContext = document.getElementById('pointsCompleted').getContext('2d');
    pointsDoughnut = new Chart(pointsDoughnutContext, {
        type: 'doughnut',
        data: {
            datasets: [{
                // INSERT POINTS EARNED AND POINTS GOAL 
                data: [],
                backgroundColor: ['#74A12E', 'grey']
            }],

            labels: [
                'Points Earned',
                'Points Remaining'
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            legend: {
                position: 'bottom'
            },
            layout: {
                padding: {
                    left: 20,
                    right: 20,
                    top: 20,
                    bottom: 20
                }
            }
        }
    });

    //Total meeting minutes
    lineChartContext = document.getElementById('meetingHours').getContext('2d');
    lineChart = new Chart(lineChartContext, {
        type: 'line',
        data: {
            datasets: [{
                label: "Meeting Minutes",
                borderColor: "#74A12E",
                pointBorderColor: "#FFF",
                pointBackgroundColor: "#74A12E",
                pointBorderWidth: 2,
                pointHoverRadius: 4,
                pointHoverBorderWidth: 1,
                pointRadius: 4,
                backgroundColor: 'transparent',
                fill: true,
                borderWidth: 2,
                // INSERT AGGREGATED DATA OF (real or fake) WEEKLY (minutes to hour) DATA
                data: []
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            legend: {
                position: 'bottom',
                labels: {
                    padding: 10,
                    fontColor: '#1d7af3',
                }
            },
            tooltips: {
                bodySpacing: 4,
                mode: "nearest",
                intersect: 0,
                position: "nearest",
                xPadding: 10,
                yPadding: 10,
                caretPadding: 10
            },
            layout: {
                padding: { left: 15, right: 15, top: 2.5, bottom: 2.5 }
            },
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'month',
                        stepSize: 1,
                        tooltipFormat: "MMM d, yyyy"
                    },
                    suggestedMin: "2023-04-01",
                    suggestedMax: "2023-05-01"
                },
                y: {
                    beginAtZero: true,
                    max: 60
                }
            }
        }
    });

    //Average Meeting Satisfaction 1-10
    satisfactionDoughnutContext = document.getElementById('meetingSatisfaction').getContext('2d');
    satisfactionDoughnut = new Chart(satisfactionDoughnutContext, {
        type: 'doughnut',
        data: {
            datasets: [{
                // INSERT TOTAL 1s (TRUE) out of TOTAL 0s (FALSE)  
                data: [],
                backgroundColor: ['grey', '#74A12E']
            }],

            labels: [
                'Unsatisfied',
                'Satisfied',
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            legend: {
                position: 'bottom'
            },
            layout: {
                padding: {
                    left: 20,
                    right: 20,
                    top: 20,
                    bottom: 20
                }
            }
        }
    });

    //Radar of effective, learning, and beneficial
    radarChartContext = document.getElementById('growthChart').getContext('2d');
    radarChart = new Chart(radarChartContext, {
        type: 'radar',
        data: {
            labels: ['Effective', 'Learning', 'Beneficial'],
            datasets: [{
                // COUNT of all TRUE in each respective category
                data: [],
                borderColor: '#74A12E',
                backgroundColor: 'rgba(116, 161, 46, 0.25)',
                pointBackgroundColor: "#74A12E",
                pointHoverRadius: 4,
                pointRadius: 3,
                label: 'Current'
            }, {
                // Design a pre-mentorship section to benchmark growth on radar
                data: [],
                borderColor: '#716aca',
                backgroundColor: 'rgba(113, 106, 202, 0.25)',
                pointBackgroundColor: "#716aca",
                pointHoverRadius: 4,
                pointRadius: 3,
                label: 'Beginning'
            },
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            legend: {
                position: 'bottom'
            }
        }
    });

    getLoggedInUserInfo();
}


// make the ajax call to get data regarding the logged in user, and change some HTML elements using that data
// however, to update the data in the charts, we will need to have a callback function that executes after AJAX call finishes.
function getLoggedInUserInfo() {
    var webMethod = "ProjectServices.asmx/getLoggedInUserInfo";
    $.ajax({
        type: "POST",
        url: webMethod,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            userInfo = JSON.parse(msg.d)
            $("#nameSpanID").append(`${userInfo.firstName} ${userInfo.lastName}`)
            $('#pointsTotal').append(`${userInfo.redeemablePoints}`)
            if (userInfo.headshotURL != "") {
                $('#loggedUserHeadshotID').attr('src', userInfo.headshotURL);
                $('#navHeadshotID').attr('src', userInfo.headshotURL);

            }
            if (userInfo.almaMaterURL != "") {
                $('#loggedUserAlmaMaterID').attr('src', userInfo.almaMaterURL);
            }

            if (userInfo.isMentor == 0) {
                $('#addMentorDivID').css("display", "block");
                $('#connectionsDropdownButtonID').text("Select Mentor")
            }
            else {
                $('#connectionsDropdownButtonID').text("Select Mentee")

            }

            updatePointsGoalChart(parseInt(userInfo.points), parseInt(userInfo.pointsGoal));
            initializeConnectionsDropdown(userInfo.connections)
        },
        error: function (e) {
            alert("Issue!...");
        }
    });

}

function updatePointsGoalChart(points, pointsGoal) {
    let pointsRemaining;

    if (points > pointsGoal) {
        pointsRemaining = 0;
    }
    else {
        pointsRemaining = pointsGoal - points
    }

    pointsDoughnut.data.datasets[0].data = [points, pointsRemaining]
    pointsDoughnut.update();

}


// Add the connections logged in user has as button in dropdown
// once clicked they will trigger a callback that calls AJAX to get data on that connection
function initializeConnectionsDropdown(connections) {
    let connectionsDropdown = document.getElementById('connectionsDropdownID');

    for (let i = 0; i < connections.length; i++) {
        let connectionButton = document.createElement('button');
        connectionButton.value = connections[i].username
        connectionButton.innerText = `${connections[i].firstName} ${connections[i].lastName}`
        connectionButton.className = 'dropdown-item custom-active-color';
        connectionButton.addEventListener('click', function () {
            getConnectionInfo(connections[i].username);
        },
        false)
        connectionsDropdown.appendChild(connectionButton);
    }
}


// takes mentor username and adds it to the mentee's list of mentors upon a button being clicked
function addMentor(mentorUsername) {
    var webMethod = "ProjectServices.asmx/addMentor";
    var parameters = "{\"mentorUsername\":\"" + encodeURI(mentorUsername) + "\"}";
    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            if (msg.d == "Success") {
                alert("Mentor added");
                window.location.reload();
            }
            else {
                alert(msg.d);
            }
        },
        error: function (e) {
            alert("Error!");
        }
    })
}


function getConnectionInfo(connectionUsername) {
    let chartsGrid = document.getElementById('chartsGridID');
    chartsGrid.style.display = 'block';

    let notesDiv = document.getElementById('notesDivID');
    notesDiv.style.display = 'block'

    var webMethod = "ProjectServices.asmx/getConnectionData";
    var parameters = "{\"connectionUsername\":\"" + encodeURI(connectionUsername) + "\"}";
    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
           let connectionInfo = JSON.parse(msg.d)
            if (connectionInfo.isMentor === 0) {
                $('#connectionInfoID').text(`Mentee: ${connectionInfo.firstName} ${connectionInfo.lastName}`);
            }
            else {
                $('#connectionInfoID').text(`Mentor: ${connectionInfo.firstName} ${connectionInfo.lastName}`);
            }

            if (connectionInfo.headshotURL != "") {
                $('#connectionHeadshotID').attr('src', connectionInfo.headshotURL);
            }
            else {
                $('#connectionHeadshotID').attr('src', "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png");
            }

            if (userInfo.almaMaterURL != "") {
                $('#connectionAlmaMaterID').attr('src', connectionInfo.almaMaterURL);
            }
            else {
                $('#connectionAlmaMaterID').attr('src', "https://seeklogo.com/images/A/asu-logo-4B38E9D815-seeklogo.com.png");
            }

            $('#collegeSpanID').text(`${connectionInfo.college}`);

            updateMeetingScheduler(connectionUsername)
            updateMeetingNotesDropdown(connectionUsername);
            updateConnectionCharts(connectionInfo.surveyResponses);
        },
        error: function (e) {
            alert("Issue!...");
        }
    });

}

// this function controls what is passed in when user schedules new meeting
function updateMeetingScheduler(connectionUsername) {
    let scheduleMeetingButton = document.getElementById('scheduleMeetingButtonID');
    // we will clone the button as a quick way to remove any existing event listeners
    // because one of the event listener arguments is the connection username, we want to ensure 
    // the correct username is passed in when the user chooses a different connection
    let newButton = scheduleMeetingButton.cloneNode(true);
    scheduleMeetingButton.parentNode.replaceChild(newButton, scheduleMeetingButton)

    // here we add the event listener for the schedule meeting button, passing in the connection username
    // the last argument will remove the event listener once triggered as well
    // technically we don't need it given the node cloning
    newButton.addEventListener("click", function () {
        scheduleMeeting(connectionUsername, $('#meetingDateInputID').val());
    }, { once: true })
}


function updateMeetingNotesDropdown(connectionUsername) {
    var webMethod = "ProjectServices.asmx/getSurveyResponseDateID";
    var parameters = "{\"connectionUsername\":\"" + encodeURI(connectionUsername) + "\"}";
    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            let surveyDateResponseIDs = JSON.parse(msg.d);
            let meetingNotesDropdownMenu = document.getElementById('meetingNotesDropdownMenuID')

            // clear all existing buttons in the dropdown for when user switches the chosen connection
            while (meetingNotesDropdownMenu.firstChild) {
                meetingNotesDropdownMenu.removeChild(meetingNotesDropdownMenu.lastChild);
            }

            for (let i = 0; i < surveyDateResponseIDs.length; i++) {
                buttonElement = document.createElement('button');
                buttonElement.className = 'dropdown-item custom-active-color';
                buttonElement.value = surveyDateResponseIDs[i].responseID;
                buttonElement.innerText = surveyDateResponseIDs[i].date;
                buttonElement.addEventListener('click', function () {
                    getSurveyMeetingNotes(surveyDateResponseIDs[i].responseID)
                })
                meetingNotesDropdownMenu.appendChild(buttonElement);
            }

        },
        error: function (e) {
            alert("Issue!...");
        }
    });
}

function scheduleMeeting(otherUsername, date) {
    var webMethod = "ProjectServices.asmx/scheduleMeeting";
    var parameters = "{\"otherUsername\":\"" + encodeURI(otherUsername) + "\",\"date\":\"" + encodeURI(date) + "\"}";
    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            if (msg.d) {
                alert("Meeting added");
            }
            else {
                alert("Meeting exists");
            }
        },
        error: function (e) {
            alert("Error!");
        }
    })
}

function getSurveyMeetingNotes(responseID) {
    var webMethod = "ProjectServices.asmx/getSurveyMeetingNotes"
    var parameters = "{\"surveyResponseID\":\"" + encodeURI(responseID) + "\"}";

    let meetingNotesDiv = document.getElementById('meetingNotesDivID');
    //Reset div for when user chooses a different meeting
    meetingNotesDiv.innerText = "";

    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            meetingNotesDiv.innerText = msg.d
        }
    })
}

function updateConnectionCharts(surveyResponses) {
    console.log(surveyResponses)
    // initialize the arrays
    let dateArray = [];
    let ratingArray = [];
    let effectivenessArray = [];
    let learningArray = [];
    let beneficialArray = [];
    let meetingLengthsArray = [];

    let dateLengthsObjArray = [];
    
    for (let i = 0; i < surveyResponses.length; i++) {
        dateArray.push(surveyResponses[i].date);
        ratingArray.push(surveyResponses[i].overallRating);
        effectivenessArray.push(surveyResponses[i].effectiveness);
        learningArray.push(surveyResponses[i].didLearn);
        beneficialArray.push(surveyResponses[i].beneficial);
        meetingLengthsArray.push(surveyResponses[i].meetingLength);
        // we need our data for the timeline chart to be formatted this way
        dateLengthsObjArray.push({ "x": surveyResponses[i].date, "y": surveyResponses[i].meetingLength})
    }

    /// adding data for the timeline chart
    lineChart.data.datasets[0].data = dateLengthsObjArray
    lineChart.update()
    // update the side card with the avg. meeting length
    $('#avgLengthSpanID').text(`${(meetingLengthsArray.reduce((sum, item) => sum + item)) / (meetingLengthsArray.length)}`)

    let sumOfRatings = 0;
    let sumOfEffectiveness = 0;
    let sumOfLearning = 0;
    let sumOfBeneficial = 0;
    
    let satisfactionDoughnutObjArray = [];
    let radarChartObjArrayOne = [];
    let radarChartObjArrayTwo = [];

    for(let i = 0; i < ratingArray.length; i++) {
        sumOfRatings += ratingArray[i];
        sumOfEffectiveness += effectivenessArray[i];
        sumOfLearning += learningArray[i];
        sumOfBeneficial += beneficialArray[i];
    }

    // overall rating (satisfaction)
    let avgRating = sumOfRatings / ratingArray.length;
    let unsatisfiedValue = 10 - avgRating;
    let satisfiedValue = avgRating; 
    let avgEffectiveness = sumOfEffectiveness / ratingArray.length;
    let avgLearning = (sumOfLearning / ratingArray.length) * 10;
    let avgBeneficial = sumOfBeneficial / ratingArray.length;

    satisfactionDoughnutObjArray.push(unsatisfiedValue);
    satisfactionDoughnutObjArray.push(satisfiedValue);
    satisfactionDoughnut.data.datasets[0].data = satisfactionDoughnutObjArray;
    satisfactionDoughnut.update();

    console.log(avgLearning);
   

    // radar chart
    radarChartObjArrayOne.push(avgEffectiveness);
    radarChartObjArrayOne.push(avgLearning);
    radarChartObjArrayOne.push(avgBeneficial);
    // we will get the initial values to show the trend has changed
    radarChartObjArrayTwo.push(effectivenessArray[0]);
    radarChartObjArrayTwo.push(learningArray[0] * 10);
    radarChartObjArrayTwo.push(beneficialArray[0]);
    radarChart.data.datasets[0].data = radarChartObjArrayOne;
    radarChart.data.datasets[1].data = radarChartObjArrayTwo;
    radarChart.update();

    $('#satisfactionSpanID').text(`${satisfiedValue}`)
    $('#learningSpanID').text(`${avgLearning}`)
    $('#avgEffectivenessSpanID').text(`${avgEffectiveness}`)
    $('#avgBeneficialSpanID').text(`${avgBeneficial}`)
}


function logout() {
    var webMethod = "ProjectServices.asmx/LogOff";
    $.ajax({
        type: "POST",
        url: webMethod,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            if (msg.d) {
                return true;
            }
            else {
                alert("Log off failed!");
            }
        },
        error: function (e) {
            alert("Issue!");
        }
    });
}