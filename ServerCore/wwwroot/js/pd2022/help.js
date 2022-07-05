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
	let helplink = document.createElement("button");
	helpspan.classList.add("online-only");
	helplink.textContent = "click here to ask the author for help";
	helplink.classList.add("help-button");
	helplink.onclick = helpClicked;
	helpspan.append(helplink);

	// Create the answer submission button if the puzzle ID is in the URL
	let params = new URLSearchParams(window.location.search);
	let hasPuzzleId = params.has("puzzleId") && (params.get("puzzleId").length > 0);
	let answerspan = document.createElement("span");
	let answerlink = document.createElement("button");
	let break1 = document.createElement("br");
	let break2 = document.createElement("br");
	if (hasPuzzleId) {
		answerspan.classList.add("online-only");
		break1.classList.add("online-only");
		break2.classList.add("online-only");
		answerlink.textContent = "SUBMIT AN ANSWER";
		answerlink.classList.add("help-button");
		answerlink.classList.add("answer-button");
		answerlink.onclick = answerClicked;
		answerspan.append(answerlink);
	}

	// Insert the buttons
	if ((childindex + 1) == authordiv.childNodes.length) {
		authordiv.insertBefore(helpspan, null);
		if (hasPuzzleId) {
			authordiv.insertBefore(break1, null);
			authordiv.insertBefore(break2, null);
			authordiv.insertBefore(answerspan, null);
		}
	}
	else {
		const child = authordiv.childNodes[childindex + 1];
		authordiv.insertBefore(helpspan, child);
		if (hasPuzzleId) {
			authordiv.insertBefore(break1, child);
			authordiv.insertBefore(break2, child);
			authordiv.insertBefore(answerspan, child);
		}
	}
});

async function helpClicked(event) {

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

// Open a new tab to the answer submission page for this puzzle
function answerClicked(event) {
	let params = new URLSearchParams(window.location.search);
	let link = document.createElement("a");
	link.href = "https://puzzlehunt.azurewebsites.net/pd2022/play/Submissions/" + params.get("puzzleId");
	link.target = "_blank";
	document.body.appendChild(link);
	link.click();
	link.remove();
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
		linkContent += "&";
	}
	linkContent += "subject=%5B";
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
