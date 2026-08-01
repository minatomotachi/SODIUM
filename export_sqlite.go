package main

import (
	"database/sql"
	"encoding/json"
	"net/http"
	"os"

	_ "modernc.org/sqlite"
)

const exportDBSchema = `
CREATE TABLE IF NOT EXISTS questions (
	id         TEXT PRIMARY KEY,
	title      TEXT NOT NULL,
	body       TEXT NOT NULL,
	tags       TEXT NOT NULL DEFAULT '[]',
	user_id    TEXT NOT NULL,
	created_at TEXT NOT NULL,
	updated_at TEXT NOT NULL,
	views      INTEGER NOT NULL DEFAULT 0,
	votes      INTEGER NOT NULL DEFAULT 0,
	answers    TEXT NOT NULL DEFAULT '[]'
);
CREATE TABLE IF NOT EXISTS answers (
	id          TEXT PRIMARY KEY,
	question_id TEXT NOT NULL,
	user_id     TEXT NOT NULL,
	body        TEXT NOT NULL,
	created_at  TEXT NOT NULL,
	updated_at  TEXT NOT NULL,
	votes       INTEGER NOT NULL DEFAULT 0,
	comments    TEXT NOT NULL DEFAULT '[]'
);
`

func handleDownloadDB(w http.ResponseWriter, r *http.Request) {
	tmp, err := os.CreateTemp("", "forum_download_*.db")
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	path := tmp.Name()
	tmp.Close()
	defer os.Remove(path)

	sqlDB, err := sql.Open("sqlite", path)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	defer sqlDB.Close()

	if _, err := sqlDB.Exec(exportDBSchema); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	questions, err := mongoListQuestions()
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	insQ, err := sqlDB.Prepare(
		`INSERT INTO questions (id, title, body, tags, user_id, created_at, updated_at, views, votes, answers)
		 VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
	)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	defer insQ.Close()

	for _, q := range questions {
		tags, _ := json.Marshal(q["tags"])
		answers, _ := json.Marshal(q["answers"])
		if _, err := insQ.Exec(
			q["_id"], q["title"], q["body"], string(tags), q["user_id"],
			q["created_at"], q["updated_at"], toInt64(q["views"]), toInt64(q["votes"]), string(answers),
		); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
	}

	answersList, err := mongoListAllAnswers()
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	insA, err := sqlDB.Prepare(
		`INSERT INTO answers (id, question_id, user_id, body, created_at, updated_at, votes, comments)
		 VALUES (?, ?, ?, ?, ?, ?, ?, ?)`,
	)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	defer insA.Close()

	for _, a := range answersList {
		comments, _ := json.Marshal(a["comments"])
		if _, err := insA.Exec(
			a["_id"], a["question_id"], a["user_id"], a["body"],
			a["created_at"], a["updated_at"], toInt64(a["votes"]), string(comments),
		); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
	}

	if _, err := sqlDB.Exec("PRAGMA wal_checkpoint(FULL);"); err != nil {
		// non-fatal: best-effort checkpoint before serving the file
	}

	w.Header().Set("Content-Type", "application/octet-stream")
	w.Header().Set("Content-Disposition", `attachment; filename="forum_download.db"`)
	http.ServeFile(w, r, path)
}

func toInt64(v interface{}) int64 {
	switch n := v.(type) {
	case int64:
		return n
	case int32:
		return int64(n)
	case int:
		return int64(n)
	case float64:
		return int64(n)
	default:
		return 0
	}
}
