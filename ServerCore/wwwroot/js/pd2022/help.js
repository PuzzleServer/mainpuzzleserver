window.addEventListener("load", function (event) {

	// Find the author's name so the button can be inserted after it
	const authordiv = document.getElementsByClassName("byline-div")[0];
	let childindex = 0;
	for (let i = 0; i < authordiv.childNodes.length; i++) {
		if ((authordiv.childNodes[i].nodeType == 3) && (authordiv.childNodes[i].textContent.trim().length > 0)) {
			childindex = i;
		}
	}

	// Create the help button
	let helpspan = document.createElement("span");
	helpspan.classList.add("help-link");
	let helplink = document.createElement("button");
	helplink.textContent = "click here";
	helplink.classList.add("help-button");
	helplink.onclick = clicked;
	helpspan.append(" (", helplink, " to ask the author for help)");

	// Insert the help button
	if ((childindex + 1) == authordiv.childNodes.length) {
		authordiv.insertBefore(helpspan, null);
	}
	else {
		const child = authordiv.childNodes[childindex + 1];
		authordiv.insertBefore(helpspan, child);
	}
});

async function clicked(event) {

	// If the URL has the necessary parameters to call the team name API, do it
	if (window.location.search.length == 0) {
		backup();
	}
	else {
		let params = new URLSearchParams(window.location.search);
		if ((!params.has("eventId")) || (!params.has("puzzleId")) || (!params.has("teamPassword"))) {
			backup();
		}
		else {
			if ((params.get("eventId").length == 0) || (params.get("puzzleId").length == 0) || (params.get("teamPassword").length == 0)) {
				backup();
			}
			else {

				// Get the team info from the API
				let response = await fetch("https://puzzlehunt.azurewebsites.net/api/puzzleapi/getmailinfo?" + params, {
					method: "GET",
					mode: "cors",
					headers: { "Content-Type": "application/json" },
					credentials: "include",
				});
				if (!response.ok) {
					backup();
				}
				else {
					let data = await response.json();
					let contactMail = data.teamContactEmail;
					let puzzleTitle = encodeURIComponent(data.puzzleName);
					let teamName = encodeURIComponent(data.teamName);
					sendMail(contactMail, puzzleTitle, teamName);
				}
			}
		}
	}
}

// If the API can't be used, scrape the puzzle title from the page
function backup() {
	let puzzleTitle = encodeURIComponent(document.getElementsByTagName("h1")[0].innerText.split("\n")[0].trim());
	sendMail("", puzzleTitle, "");
}

// Construct the mailto: link
function sendMail(contactMail, puzzleTitle, teamName) {
	let linkContent = "mailto:puzzhunt@microsoft.com?";
	if (contactMail.length > 0) {
		linkContent += "cc=";
		linkContent += contactMail;
	}
	linkContent += "&subject=%5B";
	linkContent += puzzleTitle;
	linkContent += "%5D";
	if (teamName.length > 0) {
		linkContent += "%5B";
		linkContent += teamName;
		linkContent += "%5D";
	}
	linkContent += "&body=The%20more%20details%20you%20add%20here%2C%20the%20more%20helpful%20the%20responses%20you%27ll%20get%20will%20be%21%20%F0%9F%98%87%0D%0A%0D%0A";

	// Simulate clicking it
	let email = document.createElement("a");
	email.href = linkContent;
	document.body.appendChild(email);
	email.click();
	email.remove();
}
