// Chart variables
let pointsDoughnutContext;
let pointsDoughnut;
let lineChartContext;
let lineChart;
let satisfactionDoughnutContext;
let satisfactionDoughnut;
let radarChartContext;
let radarChart;

function initialize() {
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
            labels: ["Week 1", "Week 2", "Week 3", "Week 4", "Week 5", "Week 6", "Week 7", "Week 8"],
            datasets: [{
                label: "Meeting Hours",
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
                data: [1.5, 1.25, 1.75, 2, .75, .5, .25]
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
                data: [6, 7],
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
                data: [6, 3, 7],
                borderColor: '#74A12E',
                backgroundColor: 'rgba(116, 161, 46, 0.25)',
                pointBackgroundColor: "#74A12E",
                pointHoverRadius: 4,
                pointRadius: 3,
                label: 'Current'
            }, {
                // Design a pre-mentorship section to benchmark growth on radar
                data: [2, 1, 3],
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

function getLoggedInUserInfo() {
    var webMethod = "ProjectServices.asmx/getLoggedInUserInfo";
    $.ajax({
        type: "POST",
        url: webMethod,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            userInfo = JSON.parse(msg.d)
            console.log(userInfo)
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
                $('#addMentorDivID').css("visibility", "visible");
            }

            initializePointsGoalChart(parseInt(userInfo.points), parseInt(userInfo.pointsGoal));
        },
        error: function (e) {
            alert("Issue!...");
        }
    });

}

function initializePointsGoalChart(points, pointsGoal) {
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



