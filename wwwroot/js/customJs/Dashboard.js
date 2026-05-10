

let commitsPerHourInstance = null;
let commitsPerDayInstance = null;
let charts = {};
$(document).ready(function () {
    $("#btnSubmit").on("click", handleSubmit);

    $("#btnCloseErrorModal").on("click", hideErrorModal);


});
function hideErrorModal() {
    $("#errorModal").hide();
}
function handleSubmit(e){

    e.preventDefault();
    $("#btnSubmit").prop("disabled", true);
    const repo = $("#repoInput").val();
    let commitData = null;
    $.ajax({
        url: "/Home/GetRepoData",
        type: "GET",
        data: { repo: repo },

        success: function (data) {
            commitData = data;
            console.log(data);
            $("#repoInput").val(data.repo);

            $("#TotalCommits").text(data.totalCommits);
            $("#TopContributors").text(data.topContributors.length);
            //$("#RecentCommits").text(data.recentCommits.length);

            $("#repoInput").val(data.repo);


            let totalAvg = data.weekdayCommitsAvg + data.weekendCommitsAvg;
            $("#WeekdayCommits").text(data.weekdayCommits);
            $("#WeekdayCommitsAvg").text(data.weekdayCommitsAvg);
            $("#WeekdayCommitsPercent").text((calculatePercent(data.weekdayCommitsAvg, totalAvg)) + "%");
            $("#WeekendCommits").text(data.weekendCommits);
            $("#WeekendCommitsAvg").text(data.weekendCommitsAvg);
            $("#WeekendCommitsPercent").text(calculatePercent(data.weekendCommitsAvg, totalAvg) + "%");

            let dayCommitsAvg = data.dayCommits / 9;
            let nightCommitsAvg = data.nightCommits / 15;
            let totalhourAvg = dayCommitsAvg + nightCommitsAvg;
            $("#DayCommits").text(data.dayCommits);
            $("#DayCommitsPercent").text(calculatePercent(dayCommitsAvg, totalhourAvg) + "%");
            $("#NightCommits").text(data.nightCommits);
            $("#NightCommitsPercent").text(calculatePercent(nightCommitsAvg, totalhourAvg) + "%");

            // destroy previous chart if exists
            if (commitsPerDayInstance) {
                commitsPerDayInstance.destroy();
            }
            const commitsPerDayLabels = Object.keys(data.commitsPerDay);
            const commitsPerDayValues = Object.values(data.commitsPerDay);



            renderCommitsPerDayChart(
                commitsPerDayLabels,
                commitsPerDayValues,
                "commitsChart"
            );

            // destroy previous chart if exists
            if (commitsPerHourInstance) {
                commitsPerHourInstance.destroy();
            }
            const commitsPerHourLabels = Object.keys(data.commitsPerHour);
            const commitsPerHourValues = Object.values(data.commitsPerHour);

            renderCommitsPerHourChart(
                commitsPerHourLabels,
                commitsPerHourValues,
                "commitsPerHourChart"
            );
            $("#btnSubmit").prop("disabled", false);
        },
        error: function (xhr, status, error) {

            console.log("ERROR");
            console.log(xhr);
            console.log(status);
            console.log(error);
            let message = "Something went wrong";

            if (xhr.responseText) {
                message = xhr.responseText;
            }

            $("#btnSubmit").prop("disabled", false);
            showErrorModal(message);
        }

    });

}

function showErrorModal(message) {
    document.getElementById("errorModalMessage").innerText = message;

    const modal = new bootstrap.Modal(document.getElementById("errorModal"));
    modal.show();
}

function calculatePercent(current, total) {
    let percent = Math.round(current / total * 100);
    return percent;
}

function isWeekend(dateString) {
    const date = new Date(dateString);
    const day = date.getDay(); // 0 = Sunday, 6 = Saturday
    return day === 0 || day === 6;
}
function isAfterHours(hour) {
    return hour < 9 || hour > 18;
}
function renderCommitsPerDayChart(labels, values, chartName) {
    const canvas = document.getElementById(chartName);

    if (!canvas) {
        console.error(`${chartName} canvas not found`);
        return;
    }
    const weekdayValues = labels.map((label, i) =>
        isWeekend(label) ? null : values[i]
    );

    const weekendValues = labels.map((label, i) =>
        isWeekend(label) ? values[i] : null
    );
    // Destroy previous chart if exists
    if (charts[chartName]) {
        charts[chartName].destroy();
    }

    charts[chartName] = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Weekday Commits',
                    data: weekdayValues,
                    backgroundColor: 'blue',
                    backgroundColor: 'rgba(135, 206, 250, 0.6)', // pastel blue
                    borderColor: 'rgba(135, 206, 250, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Weekend Commits',
                    data: weekendValues,
                    backgroundColor: 'rgba(255, 182, 193, 1)', // pastel red
                    borderColor: 'rgba(255, 182, 193, 1)',
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    stacked: false
                },
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}
function renderCommitsPerHourChart(labels, values, chartName) {
    const canvas = document.getElementById(chartName);

    if (!canvas) {
        console.error(`${chartName} canvas not found`);
        return;
    }
    const dayCommits = labels.map((label, i) =>
        isAfterHours(label) ? null : values[i]
    );

    const nightCommits = labels.map((label, i) =>
        isAfterHours(label) ? values[i] : null
    );
    // Destroy previous chart if exists
    if (charts[chartName]) {
        charts[chartName].destroy();
    }

    charts[chartName] = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Day Commits',
                    data: dayCommits,
                    backgroundColor: 'blue',
                    backgroundColor: 'rgba(135, 206, 250, 0.6)', // pastel blue
                    borderColor: 'rgba(135, 206, 250, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Night Commits',
                    data: nightCommits,
                    backgroundColor: 'rgba(255, 182, 193, 0.6)', // pastel red
                    borderColor: 'rgba(255, 182, 193, 1)',
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    stacked: false
                },
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}
//function renderCommitsPerHourChart(labels, values, chartName) {
//    const canvas = document.getElementById(chartName);

//    if (!canvas) {
//        console.error(`${chartName} canvas not found`);
//        return;
//    }
//    let legend = null;
//    if (chartName == "commitsPerHourChart") {
//        legend = "Commits Per Hour"
//    }
//    else {
//        legend = "Commits Per Day"
//    }
//    new Chart(canvas, {
//        type: 'line',
//        data: {
//            labels: labels,
//            datasets: [{
//                label: legend,
//                data: values,

//                // Line segment color
//                segment: {
//                    borderColor: ctx => {
//                        const index = ctx.p0DataIndex;
//                        return isAfterHours(labels[index]) ? 'red' : 'blue';
//                    }
//                },

//                // Point colors
//                pointBackgroundColor: labels.map(label =>
//                    isAfterHours(label) ? 'red' : 'blue'
//                ),

//                pointBorderColor: labels.map(label =>
//                    isAfterHours(label) ? 'red' : 'blue'
//                ),

//                borderWidth: 2,
//                tension: 0.3
//            }]
//        },

//        options: {
//            scales: {
//                x: {
//                    ticks: {
//                        color: function (context) {
//                            const label = labels[context.index];
//                            return isAfterHours(label) ? 'red' : 'blue';
//                        }
//                    }
//                }
//            }
//        }
//    });
//}
function renderCommitsChart(labels, values, chartName) {
    const canvas = document.getElementById(chartName);

    if (!canvas) {
        console.error(`${chartName} canvas not found`);
        return;
    }
    let legend = null;
    if (chartName == "commitsPerHourChart") {
        legend = "Commits Per Hour"
    }
    else {
        legend = "Commits Per Day"
    }
    new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: legend,
                data: values
            }]
        }
    });
}